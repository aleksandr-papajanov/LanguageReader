using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public sealed class ReadingItemAssetService(
    ApplicationDbContext dbContext,
    IFileStorage storage)
{
    public async Task<ReadingItemAssetFile?> OpenReadAsync(
        Guid readingItemId,
        string assetId,
        CancellationToken cancellationToken)
    {
        var asset = await ResolveAssetAsync(readingItemId, assetId, cancellationToken);
        if (asset is null)
        {
            return null;
        }

        var stream = await storage.OpenReadAsync(asset.StoragePath, cancellationToken);
        return new ReadingItemAssetFile(stream, asset.ContentType);
    }

    private Task<ReadingItemAssetEntity?> ResolveAssetAsync(
        Guid readingItemId,
        string assetId,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ReadingItemAssets
            .AsNoTracking()
            .Where(asset => asset.ReadingItemId == readingItemId);

        return string.Equals(assetId, "cover", StringComparison.OrdinalIgnoreCase)
            ? query
                .Where(asset => asset.Kind == "Image")
                .OrderByDescending(asset => asset.IsCover)
                .ThenBy(asset => asset.AssetId)
                .FirstOrDefaultAsync(cancellationToken)
            : query.FirstOrDefaultAsync(asset => asset.AssetId == assetId, cancellationToken);
    }
}

public sealed record ReadingItemAssetFile(Stream Stream, string ContentType);
