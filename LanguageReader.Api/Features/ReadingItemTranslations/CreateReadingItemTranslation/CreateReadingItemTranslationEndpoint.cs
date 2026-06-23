namespace LanguageReader.Api.Features.ReadingItemTranslations;

internal static class CreateReadingItemTranslationEndpoint
{
    public static IEndpointRouteBuilder MapCreateReadingItemTranslationEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/reading-items/{readingItemId:guid}/translations", async (
            [AsParameters] CreateTranslatedRangeRequestRoute route,
            CreateTranslatedRangeRequestBody body,
            CreateReadingItemTranslationHandler handler,
            CancellationToken ct) =>
        {
            var request = new CreateTranslatedRangeRequest(
                route.ReadingItemId,
                body.OriginalText,
                body.TranslatedText,
                body.ParagraphIndex,
                body.StartOffset,
                body.EndOffset,
                body.Username,
                body.SelectionKind,
                body.Usage);

            var result = await handler.HandleAsync(request, ct);
            return Results.Created($"/api/reading-items/{request.ReadingItemId}/translations/{result.Id}", result);
        })
        .WithName("CreateReadingItemTranslation")
        .WithOpenApi();

        return api;
    }
}
