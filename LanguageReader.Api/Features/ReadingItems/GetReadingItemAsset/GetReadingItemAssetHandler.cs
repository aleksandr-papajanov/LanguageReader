using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.ReadingItems.Services;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class GetReadingItemAssetHandler(
    ReadingItemAccessService readingItems,
    ReadingItemAssetService assets)
{
    public async Task<ReadingItemAssetResult> HandleAsync(GetReadingItemAssetRequest request, CancellationToken ct)
    {
        _ = await readingItems.LoadReadableReadOnlyAsync(request.ReadingItemId, request.Username, ct);

        var asset = await assets.OpenReadAsync(request.ReadingItemId, request.AssetId, ct)
            ?? throw new NotFoundException($"Reading item asset '{request.AssetId}' was not found.");

        return new ReadingItemAssetResult(asset.Stream, asset.ContentType);
    }
}

internal sealed record ReadingItemAssetResult(Stream Stream, string ContentType);
