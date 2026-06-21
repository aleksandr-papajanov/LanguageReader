namespace LanguageReader.Infrastructure.Features.News.Models;

public sealed record NewsArticlePreviewMetadata(
    string? Author,
    string? ImageUrl,
    DateTimeOffset? PublishedAtUtc);
