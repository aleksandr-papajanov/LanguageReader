using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class GetReadingItemAssetHandler(
    ApplicationDbContext dbContext,
    IFileStorage storage)
{
    public async Task<ReadingItemAssetResult> HandleAsync(GetReadingItemAssetRequest request, CancellationToken ct)
    {
        var item = await dbContext.ReadingItems
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == request.ReadingItemId, ct);

        if (item is null)
        {
            throw new NotFoundException($"Reading item '{request.ReadingItemId}' was not found.");
        }

        if (!ReadingItemFeatureHelpers.CanRead(item, request.Username))
        {
            throw new ForbiddenException("You do not have access to this reading item.");
        }

        var asset = await ResolveAssetAsync(request, ct)
            ?? throw new NotFoundException($"Reading item asset '{request.AssetId}' was not found.");
        var stream = await storage.OpenReadAsync(asset.StoragePath, ct);

        return new ReadingItemAssetResult(stream, asset.ContentType);
    }

    private Task<ReadingItemAssetEntity?> ResolveAssetAsync(GetReadingItemAssetRequest request, CancellationToken ct)
    {
        var query = dbContext.ReadingItemAssets
            .AsNoTracking()
            .Where(asset => asset.ReadingItemId == request.ReadingItemId);

        return string.Equals(request.AssetId, "cover", StringComparison.OrdinalIgnoreCase)
            ? query
                .Where(asset => asset.Kind == "Image")
                .OrderByDescending(asset => asset.IsCover)
                .ThenBy(asset => asset.AssetId)
                .FirstOrDefaultAsync(ct)
            : query.FirstOrDefaultAsync(asset => asset.AssetId == request.AssetId, ct);
    }
}

internal sealed record ReadingItemAssetResult(Stream Stream, string ContentType);
