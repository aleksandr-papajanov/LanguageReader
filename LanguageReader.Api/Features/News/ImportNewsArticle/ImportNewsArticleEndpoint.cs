namespace LanguageReader.Api.Features.News;

internal static class ImportNewsArticleEndpoint
{
    public static IEndpointRouteBuilder MapImportNewsArticleEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/news/articles/import", async (
            ImportNewsArticleRequest request,
            ImportNewsArticleHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return Results.Ok(result);
        });

        return api;
    }
}
