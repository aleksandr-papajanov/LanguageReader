namespace LanguageReader.Api.Features.ReadingItems;

internal static class GetReadingItemsEndpoint
{
    public static IEndpointRouteBuilder MapGetReadingItemsEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapGet("/reading-items", async (
            [AsParameters] GetReadingItemsRequest request,
            GetReadingItemsHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return Results.Ok(result);
        });

        return api;
    }
}
