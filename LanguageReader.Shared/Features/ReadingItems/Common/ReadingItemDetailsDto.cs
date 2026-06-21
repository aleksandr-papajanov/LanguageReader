namespace LanguageReader.Shared.Features.ReadingItems;

public sealed record ReadingItemDetailsDto(
    Guid Id,
    string Title,
    ReadingItemType Type,
    string OriginalLanguage,
    bool IsPublic,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string? SourceKey,
    string? SourceName,
    string? Author,
    DateTimeOffset? PublishedAtUtc,
    string? OriginalUrl,
    string? ImageUrl,
    string? Excerpt,
    string? RssFeedUrl,
    string? ExternalId);
