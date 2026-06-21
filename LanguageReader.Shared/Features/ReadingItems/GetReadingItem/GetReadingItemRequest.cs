namespace LanguageReader.Shared.Features.ReadingItems;

public sealed record GetReadingItemRequest(
    Guid ReadingItemId,
    string? Username);
