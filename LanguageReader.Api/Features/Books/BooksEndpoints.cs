namespace LanguageReader.Api.Features.Books;

internal static class BooksEndpoints
{
    public static IEndpointRouteBuilder MapBooksEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapGetBookEndpoint();
        api.MapGetBookContentEndpoint();
        api.MapCreateBookEndpoint();
        api.MapUpdateBookVisibilityEndpoint();
        api.MapDeleteBookEndpoint();

        return api;
    }
}

