namespace LanguageReader.Api.Features.Vocabulary;

internal static class AutofillVocabularyEntryEndpoint
{
    public static IEndpointRouteBuilder MapAutofillVocabularyEntryEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/vocabulary/{vocabularyId:guid}/autofill", async (
            [AsParameters] AutofillVocabularyEntryRequestRoute route,
            AutofillVocabularyEntryRequestBody body,
            AutofillVocabularyEntryHandler handler,
            CancellationToken ct) =>
        {
            var request = new AutofillVocabularyEntryRequest(
                route.VocabularyId,
                body.Username);

            return Results.Ok(await handler.HandleAsync(request, ct));
        })
        .WithName("AutofillVocabularyEntry")
        .WithOpenApi();

        return api;
    }
}
