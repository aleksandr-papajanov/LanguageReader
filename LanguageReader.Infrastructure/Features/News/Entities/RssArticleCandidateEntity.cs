using LanguageReader.Shared.Features.News;

namespace LanguageReader.Infrastructure.Features.News.Entities;

public sealed class RssArticleCandidateEntity
{
    public Guid Id { get; set; }

    public string SourceKey { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? ExternalId { get; set; }

    public DateTimeOffset? PublishedAtUtc { get; set; }

    public string? Summary { get; set; }

    public string? Author { get; set; }

    public string? ImageUrl { get; set; }

    public NewsArticleStatus Status { get; set; } = NewsArticleStatus.New;

    public Guid? SavedReadingItemId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
