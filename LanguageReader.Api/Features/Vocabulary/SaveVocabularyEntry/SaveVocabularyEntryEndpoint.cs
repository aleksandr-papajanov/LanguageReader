namespace LanguageReader.Api.Features.Vocabulary;

internal static class SaveVocabularyEntryEndpoint
{
    public static IEndpointRouteBuilder MapSaveVocabularyEntryEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/vocabulary", async (
            SaveVocabularyEntryRequest request,
            SaveVocabularyEntryHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(request, ct);
            return Results.Created($"/api/vocabulary/{result.Id}", result);
        })
        .WithName("SaveVocabulary")
        .WithOpenApi();

        return api;
    }
}

