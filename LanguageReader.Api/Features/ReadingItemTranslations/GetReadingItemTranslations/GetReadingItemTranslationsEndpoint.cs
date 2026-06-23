namespace LanguageReader.Api.Features.ReadingItemTranslations;

internal static class GetReadingItemTranslationsEndpoint
{
    public static IEndpointRouteBuilder MapGetReadingItemTranslationsEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapGet("/reading-items/{readingItemId:guid}/translations", async (
            [AsParameters] GetReadingItemTranslationsRequest request,
            GetReadingItemTranslationsHandler handler,
            CancellationToken ct) => Results.Ok(await handler.HandleAsync(request, ct)))
        .WithName("GetReadingItemTranslations")
        .WithOpenApi();

        return api;
    }
}
