namespace LanguageReader.Api.Features.Books;

internal static class GetBookContentEndpoint
{
    public static IEndpointRouteBuilder MapGetBookContentEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapGet("/books/{bookId:guid}/content", async (
            [AsParameters] GetBookContentRequest request,
            GetBookContentHandler handler,
            CancellationToken ct) => Results.Ok(await handler.HandleAsync(request, ct)))
        .WithName("GetBookContent")
        .WithOpenApi();

        return api;
    }
}
