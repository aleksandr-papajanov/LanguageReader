namespace LanguageReader.Api.Features.News;

internal static class NewsEndpoints
{
    public static IEndpointRouteBuilder MapNewsEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapPreviewNewsArticleEndpoint();
        api.MapImportNewsArticleEndpoint();

        return api;
    }
}
