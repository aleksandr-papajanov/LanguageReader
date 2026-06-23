using LanguageReader.Api.Features.Common.Mapping;
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
            entry.ReadingItem?.Title ?? "Deleted reading item",
            entry.ReadingItemId,
            entry.Username,
            entry.ReadingItemId.HasValue
                ? new ReadingPositionDto(entry.ReadingItemId.Value, entry.BlockIndex, entry.CharacterOffset)
                : null,
            entry.Kind,
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
            example.IsFromReadingItem,
            example.CreatedAtUtc,
            example.ReadingItemId,
            example.ReadingItem?.Title,
            example.ReadingItemId.HasValue && example.BlockIndex.HasValue && example.CharacterOffset.HasValue
                ? new ReadingPositionDto(example.ReadingItemId.Value, example.BlockIndex.Value, example.CharacterOffset.Value)
                : null);
    }
}
