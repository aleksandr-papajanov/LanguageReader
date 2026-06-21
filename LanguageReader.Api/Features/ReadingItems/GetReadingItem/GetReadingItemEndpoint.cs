namespace LanguageReader.Api.Features.ReadingItems;

internal static class GetReadingItemEndpoint
{
    public static IEndpointRouteBuilder MapGetReadingItemEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapGet("/reading-items/{ReadingItemId:guid}", async (
            [AsParameters] GetReadingItemRequest request,
            GetReadingItemHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return Results.Ok(result);
        });

        return api;
    }
}
