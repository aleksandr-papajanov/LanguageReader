namespace LanguageReader.Shared.Features.ReadingItems;

public sealed record GetReadingItemContentRequest(
    Guid ReadingItemId,
    string? Username);
