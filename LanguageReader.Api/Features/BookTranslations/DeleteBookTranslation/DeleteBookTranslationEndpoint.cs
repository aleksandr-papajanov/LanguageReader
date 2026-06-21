namespace LanguageReader.Api.Features.BookTranslations;

internal static class DeleteBookTranslationEndpoint
{
    public static IEndpointRouteBuilder MapDeleteBookTranslationEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapDelete("/reading-items/{readingItemId:guid}/translations/{translationId:guid}", async (
            [AsParameters] DeleteBookTranslationRequest request,
            DeleteBookTranslationHandler handler,
            CancellationToken ct) =>
        {
            await handler.HandleAsync(request, ct);
            return Results.NoContent();
        })
        .WithName("DeleteBookTranslation")
        .WithOpenApi();

        return api;
    }
}
