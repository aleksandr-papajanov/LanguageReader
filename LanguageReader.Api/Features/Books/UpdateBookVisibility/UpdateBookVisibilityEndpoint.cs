namespace LanguageReader.Api.Features.Books;

internal static class UpdateBookVisibilityEndpoint
{
    public static IEndpointRouteBuilder MapUpdateBookVisibilityEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPut("/books/{bookId:guid}/visibility", async (
            [AsParameters] UpdateBookVisibilityRequestRoute route,
            UpdateBookVisibilityRequestBody body,
            UpdateBookVisibilityHandler handler,
            CancellationToken ct) =>
        {
            var request = new UpdateBookVisibilityRequest(
                route.BookId,
                body.Username,
                body.IsPublic);

            return Results.Ok(await handler.HandleAsync(request, ct));
        })
        .WithName("UpdateBookVisibility")
        .WithOpenApi();

        return api;
    }
}


