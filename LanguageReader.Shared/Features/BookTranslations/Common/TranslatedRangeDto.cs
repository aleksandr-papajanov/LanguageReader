

namespace LanguageReader.Shared.Features.BookTranslations;

public sealed record TranslatedRangeDto(
    Guid Id,
    string Username,
    Guid ReadingItemId,
    int ParagraphIndex,
    int StartOffset,
    int EndOffset,
    string OriginalText,
    string TranslatedText,
    Guid? VocabularyEntryId,
    bool ShowOriginal,
    SavedTextKind Kind,
    DateTimeOffset CreatedAtUtc);

