namespace LanguageReader.Api.Features.Books;

internal static class DeleteBookEndpoint
{
    public static IEndpointRouteBuilder MapDeleteBookEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapDelete("/books/{id:guid}", async (
            [AsParameters] DeleteBookRequest request,
            DeleteBookHandler handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(request, ct);
            return Results.NoContent();
        })
        .WithName("DeleteBook")
        .WithOpenApi();

        return api;
    }
}
