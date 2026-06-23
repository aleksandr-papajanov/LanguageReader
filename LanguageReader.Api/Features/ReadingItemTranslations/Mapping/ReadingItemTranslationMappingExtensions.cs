using LanguageReader.Infrastructure.Features.ReadingItemTranslations.Entities;

namespace LanguageReader.Api.Features.ReadingItemTranslations;

internal static class ReadingItemTranslationMappingExtensions
{
    public static TranslatedRangeDto ToTranslatedRangeDto(this TranslatedRangeEntity range)
    {
        return new TranslatedRangeDto(
            range.Id,
            range.Username,
            range.ReadingItemId,
            range.ParagraphIndex,
            range.StartOffset,
            range.EndOffset,
            range.OriginalText,
            range.TranslatedText,
            range.VocabularyEntryId,
            range.ShowOriginal,
            range.Kind,
            range.CreatedAtUtc);
    }
}
