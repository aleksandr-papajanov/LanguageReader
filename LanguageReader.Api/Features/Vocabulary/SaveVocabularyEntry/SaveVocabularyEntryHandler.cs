using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai;
using LanguageReader.Infrastructure.Features.BookTranslations.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;
using LanguageReader.Infrastructure.Features.Vocabulary.Services.Enrichment;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class SaveVocabularyEntryHandler(
    ApplicationDbContext dbContext,
    IVocabularyEnrichmentService vocabularyEnrichmentService)
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
        var normalizedParagraphIndex = Math.Max(0, request.Position.ParagraphIndex);
        var normalizedCharacterOffset = Math.Max(0, request.Position.CharacterOffset);
        var matchingRange = await FindMatchingTranslatedRangeAsync(
            request.ReadingItemId,
            normalizedUsername,
            request.SelectionKind,
            normalizedParagraphIndex,
            normalizedCharacterOffset,
            word,
            ct);
        var normalization = request.SelectionKind == SelectionKind.Word
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
        var dictionaryForm = normalization?.DictionaryForm.Trim();
        var canonicalWord = dictionaryForm ?? word;

        var entry = matchingRange?.VocabularyEntryId is Guid linkedEntryId
            ? await LoadOwnedEntryAsync(linkedEntryId, normalizedUsername, ct)
            : null;

        entry ??= await FindExistingWordEntryAsync(
            normalizedUsername,
            request.SelectionKind,
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
            entry.SelectionKind = request.SelectionKind;
            entry.Book = readingItem;
        }
        else
        {
            entry.SourceLanguage ??= sourceLanguage;
            if (string.IsNullOrWhiteSpace(entry.Translation))
            {
                entry.Translation = translation;
            }
        }

        EnsureWordDetails(entry, word, dictionaryForm, request.SelectionKind);

        EnsureBookExample(entry, request.ContextSentence, request.Position);
        await LinkTranslatedRangeAsync(entry, matchingRange, ct);
        if (normalization is not null)
        {
            dbContext.AiOperations.Add(AiOperationMapper.ToEntity(normalization.Usage, normalizedUsername, vocabularyEntryId: entry.Id));
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
        SelectionKind selectionKind,
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
                && range.SelectionKind == selectionKind
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

    private async Task<VocabularyEntryEntity?> FindExistingWordEntryAsync(
        string username,
        SelectionKind selectionKind,
        string wordLanguage,
        string targetLanguage,
        string? dictionaryForm,
        Guid readingItemId,
        int paragraphIndex,
        int characterOffset,
        string canonicalWord,
        CancellationToken ct)
    {
        if (selectionKind == SelectionKind.Word && !string.IsNullOrWhiteSpace(dictionaryForm))
        {
            var normalizedDictionaryForm = dictionaryForm.Trim();
            var existingByDictionaryForm = await dbContext.VocabularyEntries
                .Include(item => item.Book)
                .Include(item => item.WordDetails)
                .Include(item => item.RelatedWords)
                .Include(item => item.AiOperations)
                .Include(item => item.Examples)
                    .ThenInclude(example => example.Book)
                .OrderByDescending(item => item.CreatedAtUtc)
                .FirstOrDefaultAsync(existing =>
                    existing.Username == username
                    && existing.SelectionKind == SelectionKind.Word
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
            .Include(item => item.Book)
            .Include(item => item.WordDetails)
            .Include(item => item.RelatedWords)
            .Include(item => item.AiOperations)
            .Include(item => item.Examples)
                .ThenInclude(example => example.Book)
                .FirstOrDefaultAsync(existing =>
                    existing.Username == username
                    && existing.ReadingItemId == readingItemId
                    && existing.Word.ToLower() == loweredCanonicalWord
                && existing.ParagraphIndex == paragraphIndex
                && existing.CharacterOffset == characterOffset
                && existing.SelectionKind == selectionKind
                && existing.TargetLanguage == targetLanguage,
                ct);
    }

    private static void EnsureWordDetails(
        VocabularyEntryEntity entry,
        string seenWord,
        string? dictionaryForm,
        SelectionKind selectionKind)
    {
        if (selectionKind != SelectionKind.Word)
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
            .Include(item => item.Book)
            .Include(item => item.WordDetails)
            .Include(item => item.RelatedWords)
            .Include(item => item.AiOperations)
            .Include(item => item.Examples)
                .ThenInclude(example => example.Book)
            .FirstOrDefaultAsync(item => item.Id == id && item.Username == username, ct);
    }

    private static void EnsureBookExample(VocabularyEntryEntity entry, string? contextSentence, ReadingPositionDto position)
    {
        if (entry.SelectionKind != SelectionKind.Word || string.IsNullOrWhiteSpace(contextSentence))
        {
            return;
        }

        var normalizedSentence = contextSentence.Trim();
        var existing = entry.Examples.FirstOrDefault(example =>
            example.IsFromBook
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
            IsFromBook = true,
            ReadingItemId = position.ReadingItemId,
            ParagraphIndex = position.ParagraphIndex,
            CharacterOffset = position.CharacterOffset,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
    }
}
