namespace LanguageReader.Infrastructure.Features.News.Models;

public sealed record ExtractedArticleContent(
    string SourceKey,
    string SourceName,
    string OriginalUrl,
    string Title,
    string OriginalLanguage,
    IReadOnlyList<string> Paragraphs,
    string? Author,
    string? ImageUrl,
    string? Excerpt,
    DateTimeOffset? PublishedAtUtc,
    string? ExternalId,
    string? RssFeedUrl);
