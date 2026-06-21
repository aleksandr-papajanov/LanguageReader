namespace LanguageReader.Api.Features.BookTranslations;

internal static class UpdateBookTranslationDisplayEndpoint
{
    public static IEndpointRouteBuilder MapUpdateBookTranslationDisplayEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPut("/reading-items/{readingItemId:guid}/translations/{translationId:guid}/display", async (
            [AsParameters] UpdateTranslatedRangeDisplayRequestRoute route,
            UpdateTranslatedRangeDisplayRequestBody body,
            UpdateBookTranslationDisplayHandler handler,
            CancellationToken ct) => 
            {
                var request = new UpdateTranslatedRangeDisplayRequest(
                    route.ReadingItemId,
                    route.TranslationId,
                    body.Username,
                    body.ShowOriginal);

                return Results.Ok(await handler.HandleAsync(request, ct));
            })
        .WithName("UpdateBookTranslationDisplay")
        .WithOpenApi();

        return api;
    }
}
