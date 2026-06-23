namespace LanguageReader.Shared.Features.ReadingItems;

public sealed record GetReadingItemContentRequest(
    Guid ReadingItemId,
    string? Username,
    int? PageIndex = null,
    int? BlockIndex = null,
    int? TargetPageWeight = null);
