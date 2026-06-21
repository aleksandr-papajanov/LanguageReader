namespace LanguageReader.Shared.Features.BookTranslations;

public sealed record UpdateTranslatedRangeDisplayRequest(
    Guid ReadingItemId,
    Guid TranslationId,
    string Username,
    bool ShowOriginal);

public sealed record UpdateTranslatedRangeDisplayRequestRoute(
    Guid ReadingItemId,
    Guid TranslationId);

public sealed record UpdateTranslatedRangeDisplayRequestBody(
    string Username,
    bool ShowOriginal);
