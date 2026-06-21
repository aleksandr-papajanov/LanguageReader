namespace LanguageReader.Api.Features.ReadingItems;

internal static class ReadingItemsEndpoints
{
    public static IEndpointRouteBuilder MapReadingItemsEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapGetReadingItemsEndpoint();
        api.MapGetReadingItemEndpoint();
        api.MapGetReadingItemContentEndpoint();
        api.MapUpdateReadingItemVisibilityEndpoint();
        api.MapDeleteReadingItemEndpoint();

        return api;
    }
}
