namespace LanguageReader.Infrastructure.Features.News.Models;

public sealed record FetchedNewsArticle(
    string SourceKey,
    string SourceName,
    string Title,
    string Url,
    string? Summary,
    string? ExternalId,
    DateTimeOffset? PublishedAtUtc,
    string? Author,
    string? ImageUrl,
    string RssFeedUrl);
