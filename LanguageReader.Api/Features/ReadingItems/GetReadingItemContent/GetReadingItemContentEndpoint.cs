namespace LanguageReader.Api.Features.ReadingItems;

internal static class GetReadingItemContentEndpoint
{
    public static IEndpointRouteBuilder MapGetReadingItemContentEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapGet("/reading-items/{ReadingItemId:guid}/content", async (
            [AsParameters] GetReadingItemContentRequest request,
            GetReadingItemContentHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return Results.Ok(result);
        });

        return api;
    }
}
