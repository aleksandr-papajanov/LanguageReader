using LanguageReader.Infrastructure.Features.Common.Language;
using LanguageReader.Infrastructure.Features.News.Entities;
using LanguageReader.Infrastructure.Features.Reading.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;

namespace LanguageReader.Api.Features.ReadingItems;

internal static class ReadingItemMappingExtensions
{
    public static ReadingItemDetailsDto ToReadingItemDetailsDto(
        this ReadingItemEntity item,
        string normalizedUsername,
        ReadingItemApiUrlBuilder apiUrls)
    {
        return new ReadingItemDetailsDto(
            item.Id,
            item.Title,
            item.Type,
            LanguageNameNormalizer.Normalize(item.OriginalLanguage),
            item.IsPublic,
            item.CreatedAtUtc,
            item.UpdatedAtUtc,
            ReadingItemFeatureHelpers.ResolveSourceKey(
                item.ArticleMetadata?.SourceName,
                item.ArticleMetadata?.RssFeedUrl,
                item.ArticleMetadata?.OriginalUrl),
            item.ArticleMetadata?.SourceName,
            item.ArticleMetadata?.Author,
            item.ArticleMetadata?.PublishedAtUtc,
            item.ArticleMetadata?.OriginalUrl,
            apiUrls.GetCoverImageUrl(item, normalizedUsername),
            item.ArticleMetadata?.Excerpt,
            item.ArticleMetadata?.RssFeedUrl,
            item.ArticleMetadata?.ExternalId);
    }

    public static ReadingItemSummaryDto ToReadingItemSummaryDto(
        this ReadingItemEntity item,
        string normalizedUsername,
        ReadingProgressEntity? progress,
        ReadingItemApiUrlBuilder apiUrls)
    {
        var isOwnedByCurrentUser = !string.IsNullOrWhiteSpace(normalizedUsername)
            && string.Equals(item.OwnerUsername, normalizedUsername, StringComparison.OrdinalIgnoreCase);
        var readingStatus = ReadingItemFeatureHelpers.GetReadingStatus(progress);

        return new ReadingItemSummaryDto(
            item.Id,
            item.Title,
            item.Type,
            LanguageNameNormalizer.Normalize(item.OriginalLanguage),
            ReadingItemFeatureHelpers.ResolveSourceKey(
                item.ArticleMetadata?.SourceName,
                item.ArticleMetadata?.RssFeedUrl,
                item.ArticleMetadata?.OriginalUrl),
            item.ArticleMetadata?.SourceName,
            item.ArticleMetadata?.Author,
            item.ArticleMetadata?.PublishedAtUtc,
            item.ArticleMetadata?.OriginalUrl,
            apiUrls.GetCoverImageUrl(item, normalizedUsername),
            item.ArticleMetadata?.Excerpt,
            isOwnedByCurrentUser,
            isOwnedByCurrentUser,
            item.IsPublic,
            ReadingItemCollectionFilter.Library,
            readingStatus,
            progress?.ProgressPercent,
            progress is null
                ? null
                : new ReadingPositionDto(item.Id, progress.BlockIndex, progress.CharacterOffset),
            progress?.LastOpenedAtUtc,
            null,
            true,
            progress is not null && progress.ProgressPercent > 0,
            false,
            isOwnedByCurrentUser && !item.IsPublic,
            isOwnedByCurrentUser && item.IsPublic,
            isOwnedByCurrentUser,
            !string.IsNullOrWhiteSpace(item.ArticleMetadata?.OriginalUrl),
            item.CreatedAtUtc,
            item.UpdatedAtUtc);
    }

    public static ReadingItemSummaryDto ToReadingItemSummaryDto(
        this RssArticleCandidateEntity candidate,
        string normalizedUsername,
        ReadingItemEntity? savedItem,
        ReadingProgressEntity? progress,
        ReadingItemApiUrlBuilder apiUrls)
    {
        var isOwnedByCurrentUser = savedItem is not null
            && !string.IsNullOrWhiteSpace(normalizedUsername)
            && string.Equals(savedItem.OwnerUsername, normalizedUsername, StringComparison.OrdinalIgnoreCase);
        var readingStatus = ReadingItemFeatureHelpers.GetReadingStatus(progress);
        var canOpen = savedItem is not null && ReadingItemFeatureHelpers.CanRead(savedItem, normalizedUsername);

        return new ReadingItemSummaryDto(
            savedItem?.Id ?? candidate.Id,
            savedItem?.Title ?? candidate.Title,
            savedItem?.Type ?? ReadingItemType.Article,
            savedItem is null
                ? ReadingItemFeatureHelpers.ResolveDiscoverLanguage(candidate.SourceKey)
                : LanguageNameNormalizer.Normalize(savedItem.OriginalLanguage),
            candidate.SourceKey,
            candidate.SourceName,
            savedItem?.ArticleMetadata?.Author ?? candidate.Author,
            savedItem?.ArticleMetadata?.PublishedAtUtc ?? candidate.PublishedAtUtc,
            savedItem?.ArticleMetadata?.OriginalUrl ?? candidate.Url,
            savedItem is null
                ? candidate.ImageUrl
                : apiUrls.GetCoverImageUrl(savedItem, normalizedUsername),
            savedItem?.ArticleMetadata?.Excerpt ?? candidate.Summary,
            isOwnedByCurrentUser,
            isOwnedByCurrentUser,
            savedItem?.IsPublic ?? false,
            ReadingItemCollectionFilter.Discover,
            readingStatus,
            progress?.ProgressPercent,
            savedItem is null || progress is null
                ? null
                : new ReadingPositionDto(savedItem.Id, progress.BlockIndex, progress.CharacterOffset),
            progress?.LastOpenedAtUtc,
            candidate.Status,
            canOpen,
            canOpen && progress is not null && progress.ProgressPercent > 0,
            savedItem is null,
            isOwnedByCurrentUser && savedItem is not null && !savedItem.IsPublic,
            isOwnedByCurrentUser && savedItem is not null && savedItem.IsPublic,
            isOwnedByCurrentUser && savedItem is not null,
            !string.IsNullOrWhiteSpace(candidate.Url),
            savedItem?.CreatedAtUtc ?? candidate.CreatedAtUtc,
            savedItem?.UpdatedAtUtc ?? candidate.UpdatedAtUtc);
    }
}
