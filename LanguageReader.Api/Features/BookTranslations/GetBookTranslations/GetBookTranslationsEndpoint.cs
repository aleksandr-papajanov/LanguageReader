namespace LanguageReader.Api.Features.BookTranslations;

internal static class GetBookTranslationsEndpoint
{
    public static IEndpointRouteBuilder MapGetBookTranslationsEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapGet("/reading-items/{readingItemId:guid}/translations", async (
            [AsParameters] GetBookTranslationsRequest request,
            GetBookTranslationsHandler handler,
            CancellationToken ct) => Results.Ok(await handler.HandleAsync(request, ct)))
        .WithName("GetBookTranslations")
        .WithOpenApi();

        return api;
    }
}
