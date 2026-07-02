using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.News.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public sealed class ReadingItemDeletionService(
    ApplicationDbContext dbContext,
    IFileStorage storage)
{
    public async Task DeleteAsync(ReadingItemEntity item, CancellationToken cancellationToken)
    {
        var assetStoragePaths = await dbContext.ReadingItemAssets
            .Where(asset => asset.ReadingItemId == item.Id)
            .Select(asset => asset.StoragePath)
            .ToListAsync(cancellationToken);

        var rssCandidates = await dbContext.RssArticleCandidates
            .Where(candidate => candidate.SavedReadingItemId == item.Id)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        foreach (var candidate in rssCandidates)
        {
            candidate.SavedReadingItemId = null;
            candidate.Status = NewsArticleStatus.ExtractionSucceeded;
            candidate.UpdatedAtUtc = now;
        }

        dbContext.ReadingItems.Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var storagePath in assetStoragePaths)
        {
            await storage.DeleteAsync(storagePath, cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(item.StoragePath))
        {
            await storage.DeleteAsync(item.StoragePath, cancellationToken);
        }
    }
}
