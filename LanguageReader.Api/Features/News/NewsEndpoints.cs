namespace LanguageReader.Api.Features.News;

internal static class NewsEndpoints
{
    public static IEndpointRouteBuilder MapNewsEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapImportNewsArticleEndpoint();

        return api;
    }
}
