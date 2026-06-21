using LanguageReader.Shared.Features.Common;
using LanguageReader.Shared.Features.News;

namespace LanguageReader.Shared.Features.ReadingItems;

public sealed record ReadingItemSummaryDto(
    Guid Id,
    string Title,
    ReadingItemType Type,
    string OriginalLanguage,
    string? SourceKey,
    string? SourceName,
    string? Author,
    DateTimeOffset? PublishedAtUtc,
    string? OriginalUrl,
    string? ImageUrl,
    string? Excerpt,
    bool IsOwnedByCurrentUser,
    bool IsInUserLibrary,
    bool IsPublic,
    ReadingItemCollectionFilter Collection,
    ReadingItemReadingStatus ReadingStatus,
    double? ProgressPercent,
    ReadingPositionDto? Position,
    DateTimeOffset? LastOpenedAtUtc,
    NewsArticleStatus? NewsStatus,
    bool CanOpen,
    bool CanContinue,
    bool CanImport,
    bool CanPublish,
    bool CanUnpublish,
    bool CanDelete,
    bool CanOpenOriginal,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
