using LanguageReader.Infrastructure.Features.ReadingItems.Services;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class GetReadingItemsHandler(
    ReadingItemLibraryQueryService libraryQuery,
    ReadingItemDiscoveryService discovery,
    ReadingItemApiUrlBuilder apiUrls)
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
        var rows = await libraryQuery.LoadAsync(request, normalizedUsername, ct);

        return rows
            .Select(row => row.Item.ToReadingItemSummaryDto(
                normalizedUsername,
                row.Progress,
                apiUrls))
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

        var rows = await discovery.LoadAsync(sourceKey, normalizedUsername, ct);

        return rows
            .Select(row => row.Candidate.ToReadingItemSummaryDto(
                normalizedUsername,
                row.SavedItem,
                row.Progress,
                apiUrls))
            .ToList();
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
