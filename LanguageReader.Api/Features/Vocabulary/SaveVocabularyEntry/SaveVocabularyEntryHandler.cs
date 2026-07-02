using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Infrastructure.Ai.Workflows;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.ReadingItems.Services;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Services;
using LanguageReader.Infrastructure.Features.Vocabulary.Workflows;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class SaveVocabularyEntryHandler(
    WorkflowRunner workflowRunner,
    ReadingItemAccessService readingItems,
    VocabularyEntryGraphService vocabularyEntries,
    VocabularyTranslatedRangeService translatedRanges,
    VocabularyEntrySaveService entrySave,
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

        var readingItem = await readingItems.LoadReadableAsync(request.ReadingItemId, normalizedUsername, ct);

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
        var matchingRange = await translatedRanges.FindMatchingAsync(
            request.ReadingItemId,
            normalizedUsername,
            enrichment.Kind,
            normalizedBlockIndex,
            normalizedCharacterOffset,
            word,
            ct);

        var entry = matchingRange?.VocabularyEntryId is Guid linkedEntryId
            ? await vocabularyEntries.LoadOwnedOrDefaultAsync(linkedEntryId, normalizedUsername, ct)
            : null;

        entry ??= enrichment.ExistingEntry;
        entry ??= await vocabularyEntries.FindExistingAtPositionAsync(
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
            entry = CreateEntry(normalizedUsername, request.ReadingItemId);
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
        await translatedRanges.LinkToEntryAsync(matchingRange, entry, ct);

        if (isNewEntry && enrichment.Autofill is not null)
        {
            autofillApplicator.Apply(entry, enrichment.Autofill, ct);
        }

        await entrySave.SaveAsync(entry, isNewEntry, enrichment.Normalization?.Usage, ct);

        return entry.ToVocabularyEntryDto();
    }

    private static VocabularyEntryEntity CreateEntry(string normalizedUsername, Guid readingItemId)
    {
        return new VocabularyEntryEntity
        {
            Id = Guid.NewGuid(),
            Username = normalizedUsername,
            ReadingItemId = readingItemId,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
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
