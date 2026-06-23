namespace LanguageReader.Api.Features.ReadingItemTranslations;

internal static class UpdateReadingItemTranslationDisplayEndpoint
{
    public static IEndpointRouteBuilder MapUpdateReadingItemTranslationDisplayEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPut("/reading-items/{readingItemId:guid}/translations/{translationId:guid}/display", async (
            [AsParameters] UpdateTranslatedRangeDisplayRequestRoute route,
            UpdateTranslatedRangeDisplayRequestBody body,
            UpdateReadingItemTranslationDisplayHandler handler,
            CancellationToken ct) => 
            {
                var request = new UpdateTranslatedRangeDisplayRequest(
                    route.ReadingItemId,
                    route.TranslationId,
                    body.Username,
                    body.ShowOriginal);

                return Results.Ok(await handler.HandleAsync(request, ct));
            })
        .WithName("UpdateReadingItemTranslationDisplay")
        .WithOpenApi();

        return api;
    }
}
