namespace LanguageReader.Api.Features.Books;

internal static class GetBookEndpoint
{
    public static IEndpointRouteBuilder MapGetBookEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapGet("/books/{bookId:guid}", async (
            [AsParameters] GetBookRequest request,
            GetBookHandler handler,
            CancellationToken ct) => Results.Ok(await handler.HandleAsync(request, ct)))
        .WithName("GetBook")
        .WithOpenApi();

        return api;
    }
}
