namespace LanguageReader.Api.Features.ReadingItems;

internal sealed record ImportReadingItemRequest(
    string Username,
    string Title,
    string OriginalLanguage,
    IFormFile? File);
