using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Infrastructure.Ai.Workflows;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai;
using LanguageReader.Infrastructure.Features.ReadingItemTranslations.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Services;
using LanguageReader.Infrastructure.Features.Vocabulary.Workflows;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class SaveVocabularyEntryHandler(
    ApplicationDbContext dbContext,
    WorkflowRunner workflowRunner,
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
        var enrichment = await workflowRunner.RunAsync<SaveVocabularyEntryWorkflow, SaveVocabularyEntryWorkflowRequest, SaveVocabularyEntryWorkflowResult>(
            new SaveVocabularyEntryWorkflowRequest(
                normalizedUsername,
                word,
                translation,
                sourceLanguage,
                targetLanguage,
                request.SelectionKind,
                request.ContextSentence),
            ct);
        var normalizedBlockIndex = Math.Max(0, request.Position.BlockIndex);
        var normalizedCharacterOffset = Math.Max(0, request.Position.CharacterOffset);
        var matchingRange = await FindMatchingTranslatedRangeAsync(
            request.ReadingItemId,
            normalizedUsername,
            enrichment.Kind,
            normalizedBlockIndex,
            normalizedCharacterOffset,
            word,
            ct);

        var entry = matchingRange?.VocabularyEntryId is Guid linkedEntryId
            ? await LoadOwnedEntryAsync(linkedEntryId, normalizedUsername, ct)
            : null;

        entry ??= enrichment.ExistingEntry;
        entry ??= await FindExistingEntryAtPositionAsync(
            normalizedUsername,
            enrichment.Kind,
            targetLanguage,
            request.ReadingItemId,
            normalizedBlockIndex,
            normalizedCharacterOffset,
            enrichment.CanonicalText,
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
            entry.Word = enrichment.CanonicalText;
            entry.Translation = translation;
            entry.SourceLanguage = sourceLanguage;
            entry.TargetLanguage = targetLanguage;
            entry.BlockIndex = normalizedBlockIndex;
            entry.CharacterOffset = normalizedCharacterOffset;
            entry.Kind = enrichment.Kind;
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

        EnsureWordDetails(
            entry,
            word,
            enrichment.DictionaryForm,
            enrichment.Normalization?.PartOfSpeech,
            enrichment.Kind);
        EnsureReadingItemExample(entry, request.ContextSentence, request.Position);
        await LinkTranslatedRangeAsync(entry, matchingRange, ct);

        if (enrichment.Normalization is not null)
        {
            dbContext.AiOperations.Add(AiOperationMapper.ToEntity(enrichment.Normalization.Usage, normalizedUsername, vocabularyEntryId: entry.Id));
        }

        if (isNewEntry && enrichment.Autofill is not null)
        {
            autofillApplicator.Apply(entry, enrichment.Autofill, ct);
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
        int blockIndex,
        int characterOffset,
        string word,
        CancellationToken ct)
    {
        var normalizedWord = word.Trim().ToLowerInvariant();

        var exactMatch = await dbContext.TranslatedRanges
            .OrderByDescending(range => range.CreatedAtUtc)
            .FirstOrDefaultAsync(range =>
                range.Username == username
                && range.ReadingItemId == readingItemId
                && range.BlockIndex == blockIndex
                && range.StartOffset == characterOffset
                && range.Kind == kind
                && range.OriginalText.ToLower() == normalizedWord,
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
                && range.BlockIndex == blockIndex
                && range.StartOffset == characterOffset
                && range.OriginalText.ToLower() == normalizedWord,
                ct);
    }

    private async Task<VocabularyEntryEntity?> FindExistingEntryAtPositionAsync(
        string username,
        SavedTextKind kind,
        string targetLanguage,
        Guid readingItemId,
        int blockIndex,
        int characterOffset,
        string canonicalWord,
        CancellationToken ct)
    {
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
                && existing.BlockIndex == blockIndex
                && existing.CharacterOffset == characterOffset
                && existing.Kind == kind
                && existing.TargetLanguage == targetLanguage,
                ct);
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

    private static void EnsureWordDetails(
        VocabularyEntryEntity entry,
        string seenWord,
        string? dictionaryForm,
        string? partOfSpeech,
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

        if (!string.IsNullOrWhiteSpace(partOfSpeech))
        {
            entry.WordDetails.PartOfSpeech ??= partOfSpeech;
        }

        entry.WordDetails.SeenForm ??= seenWord;

        if (!string.Equals(seenWord, entry.Word, StringComparison.OrdinalIgnoreCase))
        {
            entry.WordDetails.SeenForm = seenWord;
        }
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
            && example.BlockIndex == position.BlockIndex
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
            BlockIndex = position.BlockIndex,
            CharacterOffset = position.CharacterOffset,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });
    }
}
