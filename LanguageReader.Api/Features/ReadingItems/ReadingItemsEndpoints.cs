namespace LanguageReader.Api.Features.ReadingItems;

internal static class ReadingItemsEndpoints
{
    public static IEndpointRouteBuilder MapReadingItemsEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapGetReadingItemsEndpoint();
        api.MapGetReadingItemEndpoint();
        api.MapGetReadingItemContentEndpoint();
        api.MapGetReadingItemAssetEndpoint();
        api.MapGetRemoteImageProxyEndpoint();
        api.MapImportReadingItemEndpoint();
        api.MapPreviewReadingItemUrlImportEndpoint();
        api.MapImportReadingItemFromUrlEndpoint();
        api.MapUpdateReadingItemVisibilityEndpoint();
        api.MapDeleteReadingItemEndpoint();

        return api;
    }
}
