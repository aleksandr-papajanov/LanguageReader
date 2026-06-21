namespace LanguageReader.Api.Features.Reading;

internal static class SaveReadingProgressEndpoint
{
    public static IEndpointRouteBuilder MapSaveReadingProgressEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPut("/reading-items/{readingItemId:guid}/progress", async (
            [AsParameters] SaveReadingProgressRequestRoute route,
            SaveReadingProgressRequestBody body,
            SaveReadingProgressHandler handler,
            CancellationToken ct) =>
        {
            var request = new SaveReadingProgressRequest(
                route.ReadingItemId,
                body.Username,
                body.ProgressPercent,
                body.Position);

            return Results.Ok(await handler.HandleAsync(request, ct));
        })
        .WithName("SaveReadingProgress")
        .WithOpenApi();

        return api;
    }
}

