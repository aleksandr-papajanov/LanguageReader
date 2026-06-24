namespace LanguageReader.Api.Features.ReadingItems;

internal static class PreviewReadingItemUrlImportEndpoint
{
    public static IEndpointRouteBuilder MapPreviewReadingItemUrlImportEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/reading-items/imports/url/preview", async (
            PreviewReadingItemUrlImportRequest request,
            PreviewReadingItemUrlImportHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return Results.Ok(result);
        })
        .WithName("PreviewReadingItemUrlImport")
        .WithOpenApi();

        return api;
    }
}
