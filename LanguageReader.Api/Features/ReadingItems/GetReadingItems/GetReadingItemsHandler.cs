using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.News.Entities;
using LanguageReader.Infrastructure.Features.News.Models;
using LanguageReader.Infrastructure.Features.News.Services;
using LanguageReader.Infrastructure.Features.Reading.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class GetReadingItemsHandler(
    ApplicationDbContext dbContext,
    INewsFeedService newsFeedService,
    IArticleImportService articleImportService)
{
    public async Task<IReadOnlyList<ReadingItemSummaryDto>> HandleAsync(GetReadingItemsRequest request, CancellationToken ct)
    {
        var normalizedUsername = ReadingItemFeatureHelpers.NormalizeUsername(request.Username);
        var items = new List<ReadingItemSummaryDto>();

        if (request.Collection is ReadingItemCollectionFilter.Library or ReadingItemCollectionFilter.All)
        {
            items.AddRange(await LoadLibraryItemsAsync(request, normalizedUsername, ct));
        }

        if (request.Collection is ReadingItemCollectionFilter.Discover or ReadingItemCollectionFilter.All)
        {
            items.AddRange(await LoadDiscoverItemsAsync(request, normalizedUsername, ct));
        }

        return ApplyOrdering(ApplyPostFilters(items, request), request).ToList();
    }

    private async Task<IReadOnlyList<ReadingItemSummaryDto>> LoadLibraryItemsAsync(
        GetReadingItemsRequest request,
        string normalizedUsername,
        CancellationToken ct)
    {
        if (request.Ownership == ReadingItemOwnershipFilter.Mine && string.IsNullOrWhiteSpace(normalizedUsername))
        {
            return [];
        }

        var query = dbContext.ReadingItems
            .AsNoTracking()
            .Include(item => item.ArticleMetadata)
            .AsQueryable();

        query = request.Ownership switch
        {
            ReadingItemOwnershipFilter.Mine => query.Where(item => item.OwnerUsername == normalizedUsername),
            ReadingItemOwnershipFilter.Public => query.Where(item => item.IsPublic),
            _ => string.IsNullOrWhiteSpace(normalizedUsername)
                ? query.Where(item => item.IsPublic)
                : query.Where(item => item.IsPublic || item.OwnerUsername == normalizedUsername)
        };

        if (request.Type.HasValue)
        {
            query = query.Where(item => item.Type == request.Type.Value);
        }

        query = request.Visibility switch
        {
            ReadingItemVisibilityFilter.Public => query.Where(item => item.IsPublic),
            ReadingItemVisibilityFilter.Private => query.Where(item => !item.IsPublic),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var queryText = request.Query.Trim();
            query = query.Where(item =>
                EF.Functions.ILike(item.Title, $"%{queryText}%")
                || (item.ArticleMetadata != null && (
                    EF.Functions.ILike(item.ArticleMetadata.SourceName ?? string.Empty, $"%{queryText}%")
                    || EF.Functions.ILike(item.ArticleMetadata.Author ?? string.Empty, $"%{queryText}%")
                    || EF.Functions.ILike(item.ArticleMetadata.Excerpt ?? string.Empty, $"%{queryText}%"))));
        }

        var readingItems = await query
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ThenByDescending(item => item.CreatedAtUtc)
            .ToListAsync(ct);

        var progressByItemId = await LoadProgressByItemIdAsync(
            readingItems.Select(item => item.Id),
            normalizedUsername,
            ct);

        return readingItems
            .Select(item => item.ToReadingItemSummaryDto(
                normalizedUsername,
                progressByItemId.GetValueOrDefault(item.Id)))
            .ToList();
    }

    private async Task<IReadOnlyList<ReadingItemSummaryDto>> LoadDiscoverItemsAsync(
        GetReadingItemsRequest request,
        string normalizedUsername,
        CancellationToken ct)
    {
        var sourceKey = string.IsNullOrWhiteSpace(request.SourceKey)
            ? ReadingItemFeatureHelpers.DefaultNewsSourceKey
            : request.SourceKey.Trim().ToLowerInvariant();

        var fetched = await newsFeedService.FetchAsync(sourceKey, ct);
        var now = DateTimeOffset.UtcNow;
        var urls = fetched
            .Select(item => item.Url)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var candidates = await dbContext.RssArticleCandidates
            .Where(item => item.SourceKey == sourceKey && urls.Contains(item.Url))
            .ToListAsync(ct);

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

        await EnrichMissingCandidatePreviewAsync(candidates, sourceKey, ct);
        await dbContext.SaveChangesAsync(ct);

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
                .Where(item => savedIds.Contains(item.Id))
                .ToListAsync(ct);

        var savedItemsById = savedItems.ToDictionary(item => item.Id);
        var staleCandidates = candidates
            .Where(item => item.SavedReadingItemId.HasValue && !savedItemsById.ContainsKey(item.SavedReadingItemId.Value))
            .ToList();

        if (staleCandidates.Count > 0)
        {
            foreach (var candidate in staleCandidates)
            {
                candidate.SavedReadingItemId = null;
                candidate.Status = candidate.Status == NewsArticleStatus.Saved
                    ? NewsArticleStatus.ExtractionSucceeded
                    : candidate.Status;
                candidate.UpdatedAtUtc = now;
            }

            await dbContext.SaveChangesAsync(ct);
        }

        var progressByItemId = await LoadProgressByItemIdAsync(savedIds, normalizedUsername, ct);

        return candidates
            .Select(candidate =>
            {
                var savedItem = candidate.SavedReadingItemId.HasValue
                    ? savedItemsById.GetValueOrDefault(candidate.SavedReadingItemId.Value)
                    : null;
                var progress = savedItem is null
                    ? null
                    : progressByItemId.GetValueOrDefault(savedItem.Id);

                return candidate.ToReadingItemSummaryDto(normalizedUsername, savedItem, progress);
            })
            .ToList();
    }

    private async Task EnrichMissingCandidatePreviewAsync(
        IReadOnlyList<RssArticleCandidateEntity> candidates,
        string sourceKey,
        CancellationToken ct)
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
                CancellationToken = ct,
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

    private async Task<Dictionary<Guid, ReadingProgressEntity>> LoadProgressByItemIdAsync(
        IEnumerable<Guid> itemIds,
        string normalizedUsername,
        CancellationToken ct)
    {
        var ids = itemIds.Distinct().ToList();
        if (ids.Count == 0 || string.IsNullOrWhiteSpace(normalizedUsername))
        {
            return [];
        }

        return await dbContext.ReadingProgresses
            .AsNoTracking()
            .Where(progress => progress.Username == normalizedUsername && ids.Contains(progress.ReadingItemId))
            .ToDictionaryAsync(progress => progress.ReadingItemId, ct);
    }

    private static IEnumerable<ReadingItemSummaryDto> ApplyPostFilters(
        IEnumerable<ReadingItemSummaryDto> items,
        GetReadingItemsRequest request)
    {
        var filtered = items;

        if (request.Type.HasValue)
        {
            filtered = filtered.Where(item => item.Type == request.Type.Value);
        }

        filtered = filtered.Where(item => ReadingItemFeatureHelpers.MatchesReadingState(item.ReadingStatus, request.ReadingState));

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var query = request.Query.Trim();
            filtered = filtered.Where(item =>
                item.Title.Contains(query, StringComparison.OrdinalIgnoreCase)
                || (item.SourceName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                || (item.Author?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
                || (item.Excerpt?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        return filtered;
    }

    private static IEnumerable<ReadingItemSummaryDto> ApplyOrdering(
        IEnumerable<ReadingItemSummaryDto> items,
        GetReadingItemsRequest request)
    {
        if (request.Collection == ReadingItemCollectionFilter.Discover)
        {
            return items
                .OrderByDescending(item => item.PublishedAtUtc ?? item.UpdatedAtUtc ?? item.CreatedAtUtc)
                .ThenBy(item => item.Title);
        }

        return items
            .OrderByDescending(item => item.LastOpenedAtUtc ?? item.PublishedAtUtc ?? item.UpdatedAtUtc ?? item.CreatedAtUtc)
            .ThenByDescending(item => item.UpdatedAtUtc ?? item.CreatedAtUtc)
            .ThenBy(item => item.Title);
    }
}
