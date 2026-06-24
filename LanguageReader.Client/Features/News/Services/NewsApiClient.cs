namespace LanguageReader.Client.Features.News.Services;

public sealed class NewsApiClient(ApiClient api)
{
    public Task<ReadingItemImportPreviewDto> PreviewArticleAsync(
        PreviewNewsArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.PostAsync<ReadingItemImportPreviewDto>(
            "/api/news/articles/preview",
            request,
            cancellationToken);
    }

    public Task<ReadingItemDetailsDto> ImportArticleAsync(
        ImportNewsArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.PostAsync<ReadingItemDetailsDto>(
            "/api/news/articles/import",
            request,
            cancellationToken);
    }
}
