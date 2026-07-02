using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.News.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Services;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.News.Services;

public sealed class NewsArticleImportService(
    ApplicationDbContext dbContext,
    IArticleImportService articleImportService,
    ReadingItemImportService readingItemImport)
{
    public async Task<ReadingItemEntity> ImportAsync(
        string username,
        string sourceKey,
        string url,
        CancellationToken cancellationToken)
    {
        var normalizedSourceKey = sourceKey.Trim().ToLowerInvariant();
        var normalizedRequestUrl = url.Trim();
        var now = DateTimeOffset.UtcNow;
        var extracted = await articleImportService.ExtractAsync(sourceKey, url, cancellationToken);
        var candidate = await dbContext.RssArticleCandidates
            .FirstOrDefaultAsync(item =>
                item.SourceKey == normalizedSourceKey
                && item.Url == normalizedRequestUrl,
                cancellationToken);

        var existing = await dbContext.ReadingItems
            .Include(item => item.ArticleMetadata)
            .Include(item => item.Assets)
            .FirstOrDefaultAsync(item =>
                item.OwnerUsername == username
                && item.ArticleMetadata != null
                && item.ArticleMetadata.OriginalUrl == extracted.OriginalUrl,
                cancellationToken);

        if (existing is not null)
        {
            if (candidate is not null)
            {
                MarkCandidateSaved(candidate, existing.Id, now);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            return existing;
        }

        var readingItem = await readingItemImport.SaveArticleAsync(
            username,
            extracted,
            requestedTitle: null,
            requestedOriginalLanguage: extracted.OriginalLanguage,
            cancellationToken);

        if (candidate is not null)
        {
            MarkCandidateSaved(candidate, readingItem.Id, now);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return readingItem;
    }

    private static void MarkCandidateSaved(
        RssArticleCandidateEntity candidate,
        Guid readingItemId,
        DateTimeOffset now)
    {
        candidate.Status = NewsArticleStatus.Saved;
        candidate.SavedReadingItemId = readingItemId;
        candidate.UpdatedAtUtc = now;
    }
}
