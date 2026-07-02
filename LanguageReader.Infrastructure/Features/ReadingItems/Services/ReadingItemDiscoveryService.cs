using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.News.Entities;
using LanguageReader.Infrastructure.Features.News.Models;
using LanguageReader.Infrastructure.Features.News.Services;
using LanguageReader.Infrastructure.Features.Reading.Services;
using LanguageReader.Infrastructure.Features.ReadingItems.Models;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public sealed class ReadingItemDiscoveryService(
    ApplicationDbContext dbContext,
    INewsFeedService newsFeedService,
    IArticleImportService articleImportService,
    ReadingProgressService readingProgress)
{
    public async Task<IReadOnlyList<ReadingItemDiscoveryQueryResult>> LoadAsync(
        string sourceKey,
        string normalizedUsername,
        CancellationToken cancellationToken)
    {
        var fetched = await newsFeedService.FetchAsync(sourceKey, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var urls = fetched
            .Select(item => item.Url)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var candidates = await dbContext.RssArticleCandidates
            .Where(item => item.SourceKey == sourceKey && urls.Contains(item.Url))
            .ToListAsync(cancellationToken);

        foreach (var article in fetched)
        {
            var candidate = candidates.FirstOrDefault(item =>
                string.Equals(item.Url, article.Url, StringComparison.OrdinalIgnoreCase));

            if (candidate is null)
            {
                candidate = new RssArticleCandidateEntity
                {
                    Id = Guid.NewGuid(),
                    SourceKey = article.SourceKey,
                    CreatedAtUtc = now
                };

                dbContext.RssArticleCandidates.Add(candidate);
                candidates.Add(candidate);
            }

            ApplyFetchedArticle(candidate, article, now);
        }

        await EnrichMissingCandidatePreviewAsync(candidates, sourceKey, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var savedIds = candidates
            .Where(item => item.SavedReadingItemId.HasValue)
            .Select(item => item.SavedReadingItemId!.Value)
            .Distinct()
            .ToList();

        var savedItems = savedIds.Count == 0
            ? []
            : await dbContext.ReadingItems
                .AsNoTracking()
                .Include(item => item.ArticleMetadata)
                .Include(item => item.Assets)
                .Where(item => savedIds.Contains(item.Id))
                .ToListAsync(cancellationToken);

        var savedItemsById = savedItems.ToDictionary(item => item.Id);
        var staleCandidates = candidates
            .Where(item => item.SavedReadingItemId.HasValue && !savedItemsById.ContainsKey(item.SavedReadingItemId.Value))
            .ToList();

        if (staleCandidates.Count > 0)
        {
            foreach (var candidate in staleCandidates)
            {
                MarkCandidateUnsaved(candidate, now);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var progressByItemId = await readingProgress.LoadByItemIdAsync(savedIds, normalizedUsername, cancellationToken);

        return candidates
            .Select(candidate =>
            {
                var savedItem = candidate.SavedReadingItemId.HasValue
                    ? savedItemsById.GetValueOrDefault(candidate.SavedReadingItemId.Value)
                    : null;
                var progress = savedItem is null
                    ? null
                    : progressByItemId.GetValueOrDefault(savedItem.Id);

                return new ReadingItemDiscoveryQueryResult(candidate, savedItem, progress);
            })
            .ToList();
    }

    private async Task EnrichMissingCandidatePreviewAsync(
        IReadOnlyList<RssArticleCandidateEntity> candidates,
        string sourceKey,
        CancellationToken cancellationToken)
    {
        var candidatesToEnrich = candidates
            .Where(item => !string.IsNullOrWhiteSpace(item.Url) && string.IsNullOrWhiteSpace(item.ImageUrl))
            .OrderByDescending(item => item.PublishedAtUtc ?? item.UpdatedAtUtc)
            .ToList();

        if (candidatesToEnrich.Count == 0)
        {
            return;
        }

        var previews = new Dictionary<Guid, NewsArticlePreviewMetadata>();

        await Parallel.ForEachAsync(
            candidatesToEnrich,
            new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 6
            },
            async (candidate, token) =>
            {
                var preview = await articleImportService.TryExtractPreviewAsync(sourceKey, candidate.Url, token);
                if (preview is null)
                {
                    return;
                }

                lock (previews)
                {
                    previews[candidate.Id] = preview;
                }
            });

        foreach (var candidate in candidatesToEnrich)
        {
            if (!previews.TryGetValue(candidate.Id, out var preview))
            {
                continue;
            }

            candidate.Author ??= preview.Author;
            candidate.ImageUrl ??= preview.ImageUrl;
            candidate.PublishedAtUtc ??= preview.PublishedAtUtc;
        }
    }

    private static void ApplyFetchedArticle(
        RssArticleCandidateEntity candidate,
        FetchedNewsArticle article,
        DateTimeOffset now)
    {
        candidate.SourceName = article.SourceName;
        candidate.Title = article.Title;
        candidate.Url = article.Url;
        candidate.ExternalId = article.ExternalId;
        candidate.PublishedAtUtc = article.PublishedAtUtc?.ToUniversalTime();
        candidate.Summary = article.Summary;
        candidate.Author = article.Author ?? candidate.Author;
        candidate.ImageUrl = article.ImageUrl ?? candidate.ImageUrl;
        candidate.UpdatedAtUtc = now;
    }

    private static void MarkCandidateUnsaved(RssArticleCandidateEntity candidate, DateTimeOffset now)
    {
        candidate.SavedReadingItemId = null;
        candidate.Status = candidate.Status == NewsArticleStatus.Saved
            ? NewsArticleStatus.ExtractionSucceeded
            : candidate.Status;
        candidate.UpdatedAtUtc = now;
    }
}
