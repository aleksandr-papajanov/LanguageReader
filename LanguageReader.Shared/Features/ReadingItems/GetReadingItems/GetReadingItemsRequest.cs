namespace LanguageReader.Shared.Features.ReadingItems;

public sealed record GetReadingItemsRequest(
    string? Username,
    ReadingItemType? Type = null,
    ReadingItemOwnershipFilter Ownership = ReadingItemOwnershipFilter.All,
    ReadingItemReadingStateFilter ReadingState = ReadingItemReadingStateFilter.All,
    ReadingItemVisibilityFilter Visibility = ReadingItemVisibilityFilter.All,
    ReadingItemCollectionFilter Collection = ReadingItemCollectionFilter.Library,
    string? SourceKey = null,
    string? Query = null);
