namespace LanguageReader.Infrastructure.Features.ReadingItems.Entities;

public sealed class ArticleMetadataEntity
{
    public Guid ReadingItemId { get; set; }

    public string SourceName { get; set; } = string.Empty;

    public string OriginalUrl { get; set; } = string.Empty;

    public DateTimeOffset? PublishedAtUtc { get; set; }

    public string? Author { get; set; }

    public string? ImageUrl { get; set; }

    public string? Excerpt { get; set; }

    public string? RssFeedUrl { get; set; }

    public string? ExternalId { get; set; }

    public ReadingItemEntity? ReadingItem { get; set; }
}
