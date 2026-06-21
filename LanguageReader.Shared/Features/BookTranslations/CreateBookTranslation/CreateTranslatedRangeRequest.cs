namespace LanguageReader.Shared.Features.BookTranslations;

public sealed record CreateTranslatedRangeRequest(
    Guid ReadingItemId,
    string OriginalText,
    string TranslatedText,
    SelectionKind? ResolvedSelectionKind,
    int ParagraphIndex,
    int StartOffset,
    int EndOffset,
    string Username,
    SelectionKind SelectionKind,
    AiOperationUsageDto? Usage);

public sealed record CreateTranslatedRangeRequestRoute(Guid ReadingItemId);

public sealed record CreateTranslatedRangeRequestBody(
    string OriginalText,
    string TranslatedText,
    SelectionKind? ResolvedSelectionKind,
    int ParagraphIndex,
    int StartOffset,
    int EndOffset,
    string Username,
    SelectionKind SelectionKind,
    AiOperationUsageDto? Usage);
