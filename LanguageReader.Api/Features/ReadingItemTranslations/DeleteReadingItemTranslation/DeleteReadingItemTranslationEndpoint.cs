namespace LanguageReader.Api.Features.ReadingItemTranslations;

internal static class DeleteReadingItemTranslationEndpoint
{
    public static IEndpointRouteBuilder MapDeleteReadingItemTranslationEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapDelete("/reading-items/{readingItemId:guid}/translations/{translationId:guid}", async (
            [AsParameters] DeleteReadingItemTranslationRequest request,
            DeleteReadingItemTranslationHandler handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(request, ct);
            return Results.NoContent();
        })
        .WithName("DeleteReadingItemTranslation")
        .WithOpenApi();

        return api;
    }
}
