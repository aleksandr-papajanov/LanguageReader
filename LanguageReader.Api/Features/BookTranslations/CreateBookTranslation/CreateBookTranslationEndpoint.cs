namespace LanguageReader.Api.Features.BookTranslations;

internal static class CreateBookTranslationEndpoint
{
    public static IEndpointRouteBuilder MapCreateBookTranslationEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/reading-items/{readingItemId:guid}/translations", async (
            [AsParameters] CreateTranslatedRangeRequestRoute route,
            CreateTranslatedRangeRequestBody body,
            CreateBookTranslationHandler handler,
            CancellationToken ct) =>
        {
            var request = new CreateTranslatedRangeRequest(
                route.ReadingItemId,
                body.OriginalText,
                body.TranslatedText,
                body.ResolvedSelectionKind,
                body.ParagraphIndex,
                body.StartOffset,
                body.EndOffset,
                body.Username,
                body.SelectionKind,
                body.Usage);

            var result = await handler.HandleAsync(request, ct);
            return Results.Created($"/api/reading-items/{request.ReadingItemId}/translations/{result.Id}", result);
        })
        .WithName("CreateBookTranslation")
        .WithOpenApi();

        return api;
    }
}
