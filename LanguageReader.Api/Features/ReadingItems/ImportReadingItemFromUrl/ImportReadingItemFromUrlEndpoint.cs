namespace LanguageReader.Api.Features.ReadingItems;

internal static class ImportReadingItemFromUrlEndpoint
{
    public static IEndpointRouteBuilder MapImportReadingItemFromUrlEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/reading-items/imports/url", async (
            ImportReadingItemFromUrlRequest request,
            ImportReadingItemFromUrlHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return Results.Created($"/api/reading-items/{result.Id}", result);
        })
        .WithName("ImportReadingItemFromUrl")
        .WithOpenApi();

        return api;
    }
}
