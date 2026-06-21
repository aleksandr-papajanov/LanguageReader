namespace LanguageReader.Api.Features.ReadingItems;

internal static class DeleteReadingItemEndpoint
{
    public static IEndpointRouteBuilder MapDeleteReadingItemEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapDelete("/reading-items/{ReadingItemId:guid}", async (
            [AsParameters] DeleteReadingItemRequest request,
            DeleteReadingItemHandler handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(request, ct);
            return Results.NoContent();
        });

        return api;
    }
}
