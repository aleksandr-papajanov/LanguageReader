namespace LanguageReader.Infrastructure.Features.News.Models;

public sealed record NewsFeedSourceDefinition(
    string SourceKey,
    string SourceName,
    string RssFeedUrl,
    string DefaultLanguage);
