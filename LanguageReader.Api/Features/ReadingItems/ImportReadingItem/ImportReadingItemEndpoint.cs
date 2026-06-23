namespace LanguageReader.Api.Features.ReadingItems;

internal static class ImportReadingItemEndpoint
{
    public static IEndpointRouteBuilder MapImportReadingItemEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/reading-items/imports/fb2", async (
            HttpRequest request,
            ImportReadingItemHandler handler,
            CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest(new { error = "Multipart form data is required." });
            }

            var form = await request.ReadFormAsync(ct);
            var importRequest = new ImportReadingItemRequest(
                form["username"].ToString(),
                form["title"].ToString(),
                form["originalLanguage"].ToString(),
                form.Files.GetFile("file"));

            var result = await handler.HandleAsync(importRequest, ct);
            return Results.Created($"/api/reading-items/{result.Id}", result);
        })
        .DisableAntiforgery()
        .WithName("ImportFb2ReadingItem")
        .WithOpenApi();

        return api;
    }
}
