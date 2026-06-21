using LanguageReader.Infrastructure.Features.Ai.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;

namespace LanguageReader.Api.Features.Vocabulary;

internal static class VocabularyMappingExtensions
{
    public static VocabularyEntryDto ToVocabularyEntryDto(this VocabularyEntryEntity entry)
    {
        return new VocabularyEntryDto(
            entry.Id,
            entry.Word,
            entry.Translation,
            entry.SourceLanguage ?? string.Empty,
            entry.TargetLanguage,
            entry.Book?.Title ?? "Deleted reading item",
            entry.ReadingItemId,
            entry.Username,
            entry.ReadingItemId.HasValue
                ? new ReadingPositionDto(entry.ReadingItemId.Value, entry.ParagraphIndex, entry.CharacterOffset)
                : null,
            entry.SelectionKind,
            entry.WordDetails?.ToVocabularyWordDetailsDto(),
            entry.RelatedWords
                .OrderBy(item => item.SortOrder)
                .Select(item => item.ToRelatedWordDto())
                .ToList(),
            entry.Examples
                .Select(item => item.ToVocabularyExampleDto())
                .ToList(),
            entry.AiOperations.ToAiUsageSummaryDto(),
            entry.AiOperations
                .OrderByDescending(item => item.CreatedAtUtc)
                .Select(item => item.ToAiOperationDto())
                .ToList(),
            entry.IsVisibleInVocabulary,
            entry.CreatedAtUtc);
    }

    public static VocabularyWordDetailsDto ToVocabularyWordDetailsDto(this VocabularyWordDetailsEntity details)
    {
        return new VocabularyWordDetailsDto(
            details.SeenForm,
            details.DictionaryForm,
            details.PartOfSpeech,
            details.Description,
            details.FrequencyScore,
            details.Notes);
    }

    public static RelatedWordDto ToRelatedWordDto(this RelatedWordEntity relatedWord)
    {
        return new RelatedWordDto(
            relatedWord.Id,
            relatedWord.Word,
            relatedWord.Type);
    }

    public static VocabularyExampleDto ToVocabularyExampleDto(this VocabularyExampleEntity example)
    {
        return new VocabularyExampleDto(
            example.Id,
            example.Text,
            example.Translation,
            example.IsFromBook,
            example.CreatedAtUtc,
            example.ReadingItemId,
            example.Book?.Title,
            example.ReadingItemId.HasValue && example.ParagraphIndex.HasValue && example.CharacterOffset.HasValue
                ? new ReadingPositionDto(example.ReadingItemId.Value, example.ParagraphIndex.Value, example.CharacterOffset.Value)
                : null);
    }

    public static AiOperationDto ToAiOperationDto(this AiOperationEntity operation)
    {
        return new AiOperationDto(
            operation.Id,
            operation.Kind,
            operation.Provider,
            operation.Model,
            operation.InputTokens,
            operation.OutputTokens,
            operation.TotalTokens,
            operation.InputCostUsd,
            operation.OutputCostUsd,
            operation.TotalCostUsd,
            operation.CreatedAtUtc);
    }

    public static AiUsageSummaryDto ToAiUsageSummaryDto(this IEnumerable<AiOperationEntity> operations)
    {
        var items = operations.ToList();

        return new AiUsageSummaryDto(
            items.Count,
            items.Sum(item => item.InputTokens),
            items.Sum(item => item.OutputTokens),
            items.Sum(item => item.TotalTokens),
            items.Sum(item => item.TotalCostUsd));
    }
}
