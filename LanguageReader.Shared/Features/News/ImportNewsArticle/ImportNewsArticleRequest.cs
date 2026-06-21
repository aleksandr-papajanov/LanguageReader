namespace LanguageReader.Shared.Features.News;

public sealed record ImportNewsArticleRequest(
    string Username,
    string SourceKey,
    string Url);
