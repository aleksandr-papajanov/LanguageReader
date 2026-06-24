namespace LanguageReader.Shared.Features.News;

public sealed record PreviewNewsArticleRequest(
    string SourceKey,
    string Url);
