namespace LanguageReader.Shared.Features.ReadingItems;

public sealed record DeleteReadingItemRequest(
    Guid ReadingItemId,
    string Username);
