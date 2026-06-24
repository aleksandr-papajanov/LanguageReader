namespace LanguageReader.Api.Features.ReadingItems;

internal static class GetReadingItemAssetEndpoint
{
    public static IEndpointRouteBuilder MapGetReadingItemAssetEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapGet("/reading-items/{ReadingItemId:guid}/assets/{AssetId}", async (
            [AsParameters] GetReadingItemAssetRequest request,
            GetReadingItemAssetHandler handler,
            CancellationToken ct) =>
        {
            var asset = await handler.HandleAsync(request, ct);
            return Results.File(asset.Stream, asset.ContentType);
        });

        return api;
    }
}
