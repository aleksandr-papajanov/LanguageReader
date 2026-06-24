namespace LanguageReader.Shared.Features.ReadingItems;

public sealed record ImportReadingItemFromUrlRequest(
    string Username,
    string Url,
    string Title,
    string OriginalLanguage);
