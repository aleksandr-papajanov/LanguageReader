namespace LanguageReader.Api.Features.News;

internal static class PreviewNewsArticleEndpoint
{
    public static IEndpointRouteBuilder MapPreviewNewsArticleEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/news/articles/preview", async (
            PreviewNewsArticleRequest request,
            PreviewNewsArticleHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return Results.Ok(result);
        })
        .WithName("PreviewNewsArticle")
        .WithOpenApi();

        return api;
    }
}
