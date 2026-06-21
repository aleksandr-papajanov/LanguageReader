

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
    SelectionKind ResolvedSelectionKind,
    Guid? VocabularyEntryId,
    bool ShowOriginal,
    SelectionKind SelectionKind,
    DateTimeOffset CreatedAtUtc);

