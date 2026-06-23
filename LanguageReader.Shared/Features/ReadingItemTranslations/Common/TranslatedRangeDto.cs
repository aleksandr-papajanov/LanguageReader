

namespace LanguageReader.Shared.Features.ReadingItemTranslations;

public sealed record TranslatedRangeDto(
    Guid Id,
    string Username,
    Guid ReadingItemId,
    int BlockIndex,
    int StartOffset,
    int EndOffset,
    string OriginalText,
    string TranslatedText,
    Guid? VocabularyEntryId,
    bool ShowOriginal,
    SavedTextKind Kind,
    DateTimeOffset CreatedAtUtc);

