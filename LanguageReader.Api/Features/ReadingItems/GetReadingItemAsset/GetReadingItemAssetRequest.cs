namespace LanguageReader.Api.Features.ReadingItems;

internal sealed record GetReadingItemAssetRequest(
    Guid ReadingItemId,
    string AssetId,
    string? Username);
