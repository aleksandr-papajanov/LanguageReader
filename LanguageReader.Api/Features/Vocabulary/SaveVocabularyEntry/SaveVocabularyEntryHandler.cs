using LanguageReader.Api.Features.Vocabulary.Services;
using LanguageReader.Api.Features.Common.Services;
using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai;
using LanguageReader.Infrastructure.Features.ReadingItemTranslations.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;
using LanguageReader.Infrastructure.Features.Vocabulary.Services.Enrichment;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class SaveVocabularyEntryHandler(
    ApplicationDbContext dbContext,
    IVocabularyEnrichmentService vocabularyEnrichmentService,
    VocabularyAutofillApplicator autofillApplicator)
{
    public async Task<VocabularyEntryDto> HandleAsync(SaveVocabularyEntryRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);

        if (string.IsNullOrWhiteSpace(request.Word))
        {
            throw new ValidationException("Text is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Translation))
        {
            throw new ValidationException("Translation is required.");
        }

        if (string.IsNullOrWhiteSpace(request.TargetLanguage))
        {
            throw new ValidationException("Target language is required.");
        }

        var readingItem = await dbContext.ReadingItems.FirstOrDefaultAsync(item => item.Id == request.ReadingItemId, ct);
        if (readingItem is null)
        {
            throw new NotFoundException($"Reading item '{request.ReadingItemId}' was not found.");
        }

        if (!ReadingItemFeatureHelpers.CanRead(readingItem, normalizedUsername))
        {
            throw new ForbiddenException("You do not have access to this reading item.");
        }

        var word = request.Word.Trim();
        var translation = request.Translation.Trim();
        var sourceLanguage = string.IsNullOrWhiteSpace(request.SourceLanguage) ? null : request.SourceLanguage.Trim();
        var targetLanguage = request.TargetLanguage.Trim();
        var wordLanguage = string.IsNullOrWhiteSpace(sourceLanguage) ? targetLanguage : sourceLanguage;
        var shouldResolveCandidate = request.SelectionKind is SelectionKind.Word or SelectionKind.Unknown;
        var candidate = shouldResolveCandidate
            ? await vocabularyEnrichmentService.NormalizeAsync(
                new VocabularyNormalizationRequest(
                    normalizedUsername,
                    word,
                    translation,
                    wordLanguage,
                    targetLanguage,
                    request.ContextSentence),
                ct)
            : null;
        var kind = request.SelectionKind == SelectionKind.Word || candidate?.IsLexicalUnit == true
            ? SavedTextKind.LexicalUnit
            : SavedTextKindMapper.FromSelectionKind(request.SelectionKind);
        var normalizedParagraphIndex = Math.Max(0, request.Position.ParagraphIndex);
        var normalizedCharacterOffset = Math.Max(0, request.Position.CharacterOffset);
        var matchingRange = await FindMatchingTranslatedRangeAsync(
            request.ReadingItemId,
            normalizedUsername,
            kind,
            normalizedParagraphIndex,
            normalizedCharacterOffset,
            word,
            ct);
        var dictionaryForm = kind == SavedTextKind.LexicalUnit && !string.IsNullOrWhiteSpace(candidate?.DictionaryForm)
            ? candidate.DictionaryForm.Trim()
            : null;
        var canonicalWord = dictionaryForm ?? word;

        var entry = matchingRange?.VocabularyEntryId is Guid linkedEntryId
            ? await LoadOwnedEntryAsync(linkedEntryId, normalizedUsername, ct)
            : null;

        entry ??= await FindExistingEntryAsync(
            normalizedUsername,
            kind,
            wordLanguage,
            targetLanguage,
            dictionaryForm,
            request.ReadingItemId,
            normalizedParagraphIndex,
            normalizedCharacterOffset,
            canonicalWord,
            ct);

        var isNewEntry = entry is null;

        if (isNewEntry)
        {
            entry = new VocabularyEntryEntity
            {
                Id = Guid.NewGuid(),
                Username = normalizedUsername,
                ReadingItemId = request.ReadingItemId,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            dbContext.VocabularyEntries.Add(entry);
        }

        if (entry is null)
        {
            throw new InvalidOperationException("Vocabulary entry resolution failed.");
        }

        entry.IsVisibleInVocabulary = request.IsVisibleInVocabulary;

        if (isNewEntry)
        {
            entry.Word = canonicalWord;
            entry.Translation = translation;
            entry.SourceLanguage = sourceLanguage;
            entry.TargetLanguage = targetLanguage;
            entry.ParagraphIndex = normalizedParagraphIndex;
            entry.CharacterOffset = normalizedCharacterOffset;
            entry.Kind = kind;
            entry.ReadingItem = readingItem;
        }
        else
        {
            entry.SourceLanguage ??= sourceLanguage;
            if (string.IsNullOrWhiteSpace(entry.Translation))
            {
                entry.Translation = translation;
            }
        }

        EnsureWordDetails(entry, word, dictionaryForm, kind);

        EnsureReadingItemExample(entry, request.ContextSentence, request.Position);
        await LinkTranslatedRangeAsync(entry, matchingRange, ct);
        if (candidate is not null)
        {
            dbContext.AiOperations.Add(AiOperationMapper.ToEntity(candidate.Usage, normalizedUsername, vocabularyEntryId: entry.Id));
        }

        if (kind == SavedTextKind.LexicalUnit)
        {
            var generated = await vocabularyEnrichmentService.AutofillAsync(
                new VocabularyAutofillRequest(
                    normalizedUsername,
                    entry.Word,
                    entry.Translation,
                    wordLanguage,
                    targetLanguage,
                    request.ContextSentence),
                ct);

            autofillApplicator.Apply(entry, generated, ct);
        }

        await dbContext.SaveChangesAsync(ct);

        return entry.ToVocabularyEntryDto();
    }

    private async Task LinkTranslatedRangeAsync(
        VocabularyEntryEntity entry,
        TranslatedRangeEntity? matchingRange,
        CancellationToken ct)
    {
        if (matchingRange is null)
        {
            return;
        }

        matchingRange.VocabularyEntryId = entry.Id;
        matchingRange.Kind = entry.Kind;

        await dbContext.Entry(matchingRange)
            .Collection(range => range.AiOperations)
            .LoadAsync(ct);

        foreach (var operation in matchingRange.AiOperations.Where(item => item.VocabularyEntryId != entry.Id))
        {
            operation.VocabularyEntryId = entry.Id;
        }
    }

    private async Task<TranslatedRangeEntity?> FindMatchingTranslatedRangeAsync(
        Guid readingItemId,
        string username,
        SavedTextKind kind,
        int paragraphIndex,
        int characterOffset,
        string word,
        CancellationToken ct)
    {
        var exactMatch = await dbContext.TranslatedRanges
            .OrderByDescending(range => range.CreatedAtUtc)
            .FirstOrDefaultAsync(range =>
                range.Username == username
                && range.ReadingItemId == readingItemId
                && range.ParagraphIndex == paragraphIndex
                && range.StartOffset == characterOffset
                && range.Kind == kind
                && range.OriginalText == word,
                ct);

        if (exactMatch is not null)
        {
            return exactMatch;
        }

        return await dbContext.TranslatedRanges
            .OrderByDescending(range => range.CreatedAtUtc)
            .FirstOrDefaultAsync(range =>
                range.Username == username
                && range.ReadingItemId == readingItemId
                && range.ParagraphIndex == paragraphIndex
                && range.StartOffset == characterOffset
                && range.OriginalText == word,
                ct);
    }

    private async Task<VocabularyEntryEntity?> FindExistingEntryAsync(
        string username,
        SavedTextKind kind,
        string wordLanguage,
        string targetLanguage,
        string? dictionaryForm,
        Guid readingItemId,
        int paragraphIndex,
        int characterOffset,
        string canonicalWord,
        CancellationToken ct)
    {
        if (kind == SavedTextKind.LexicalUnit && !string.IsNullOrWhiteSpace(dictionaryForm))
        {
            var normalizedDictionaryForm = dictionaryForm.Trim();
            var existingByDictionaryForm = await dbContext.VocabularyEntries
                .Include(item => item.ReadingItem)
                .Include(item => item.WordDetails)
                .Include(item => item.RelatedWords)
                .Include(item => item.AiOperations)
                .Include(item => item.Examples)
                    .ThenInclude(example => example.ReadingItem)
                .OrderByDescending(item => item.CreatedAtUtc)
                .FirstOrDefaultAsync(existing =>
                    existing.Username == username
                    && existing.Kind == SavedTextKind.LexicalUnit
                    && existing.TargetLanguage == targetLanguage
                    && (existing.SourceLanguage ?? existing.TargetLanguage) == wordLanguage
                    && ((existing.WordDetails != null && existing.WordDetails.DictionaryForm == normalizedDictionaryForm)
                        || existing.Word == normalizedDictionaryForm),
                    ct);

            if (existingByDictionaryForm is not null)
            {
                return existingByDictionaryForm;
            }
        }

        var loweredCanonicalWord = canonicalWord.ToLowerInvariant();

        return await dbContext.VocabularyEntries
            .Include(item => item.ReadingItem)
            .Include(item => item.WordDetails)
            .Include(item => item.RelatedWords)
            .Include(item => item.AiOperations)
            .Include(item => item.Examples)
                .ThenInclude(example => example.ReadingItem)
                .FirstOrDefaultAsync(existing =>
                    existing.Username == username
                    && existing.ReadingItemId == readingItemId
                    && existing.Word.ToLower() == loweredCanonicalWord
                && existing.ParagraphIndex == paragraphIndex
                && existing.CharacterOffset == characterOffset
                && existing.Kind == kind
                && existing.TargetLanguage == targetLanguage,
                ct);
    }

    private static void EnsureWordDetails(
        VocabularyEntryEntity entry,
        string seenWord,
        string? dictionaryForm,
        SavedTextKind kind)
    {
        if (kind != SavedTextKind.LexicalUnit)
        {
            return;
        }

        entry.WordDetails ??= new VocabularyWordDetailsEntity
        {
            VocabularyEntryId = entry.Id
        };

        if (!string.IsNullOrWhiteSpace(dictionaryForm))
        {
            entry.WordDetails.DictionaryForm ??= dictionaryForm;
        }

        entry.WordDetails.SeenForm ??= seenWord;

        if (!string.Equals(seenWord, entry.Word, StringComparison.OrdinalIgnoreCase))
        {
            entry.WordDetails.SeenForm = seenWord;
        }
    }

    private async Task<VocabularyEntryEntity?> LoadOwnedEntryAsync(Guid id, string username, CancellationToken ct)
    {
        return await dbContext.VocabularyEntries
            .Include(item => item.ReadingItem)
            .Include(item => item.WordDetails)
            .Include(item => item.RelatedWords)
            .Include(item => item.AiOperations)
            .Include(item => item.Examples)
                .ThenInclude(example => example.ReadingItem)
            .FirstOrDefaultAsync(item => item.Id == id && item.Username == username, ct);
    }

    private static void EnsureReadingItemExample(VocabularyEntryEntity entry, string? contextSentence, ReadingPositionDto position)
    {
        if (entry.Kind != SavedTextKind.LexicalUnit || string.IsNullOrWhiteSpace(contextSentence))
        {
            return;
        }

        var normalizedSentence = contextSentence.Trim();
        var existing = entry.Examples.FirstOrDefault(example =>
            example.IsFromReadingItem
            && example.ReadingItemId == position.ReadingItemId
            && example.ParagraphIndex == position.ParagraphIndex
            && example.CharacterOffset == position.CharacterOffset);

        if (existing is not null)
        {
            existing.Text = normalizedSentence;
            return;
        }

        entry.Examples.Add(new VocabularyExampleEntity
        {
            Id = Guid.NewGuid(),
            VocabularyEntryId = entry.Id,
            Text = normalizedSentence,
            Translation = null,
            IsFromReadingItem = true,
            ReadingItemId = position.ReadingItemId,
            ParagraphIndex = position.ParagraphIndex,
            CharacterOffset = position.CharacterOffset,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
    }
}
