namespace LanguageReader.Api.Features.Books;

internal static class CreateBookEndpoint
{
    public static IEndpointRouteBuilder MapCreateBookEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/books", async (
            HttpRequest request,
            CreateBookHandler handler,
            CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest(new { error = "Multipart form data is required." });
            }

            var form = await request.ReadFormAsync(ct);
            var bookRequest = new CreateBookRequest(
                form["username"].ToString(),
                form["title"].ToString(),
                form["originalLanguage"].ToString(),
                form.Files.GetFile("file"));

            var result = await handler.HandleAsync(bookRequest, ct);
            return Results.Created($"/api/books/{result.Id}", result);
        })
        .DisableAntiforgery()
        .WithName("UploadBook")
        .WithOpenApi();

        return api;
    }
}
