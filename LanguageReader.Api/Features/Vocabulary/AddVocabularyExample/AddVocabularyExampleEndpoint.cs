namespace LanguageReader.Api.Features.Vocabulary;

internal static class AddVocabularyExampleEndpoint
{
    public static IEndpointRouteBuilder MapAddVocabularyExampleEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/vocabulary/{vocabularyId:guid}/examples", async (
            [AsParameters] AddVocabularyExampleRequestRoute route,
            AddVocabularyExampleRequestBody body,
            AddVocabularyExampleHandler handler,
            CancellationToken ct) =>
        {
            var request = new AddVocabularyExampleRequest(
                route.VocabularyId,
                body.Username);

            return Results.Ok(await handler.HandleAsync(request, ct));
        })
        .WithName("AddVocabularyExample")
        .WithOpenApi();

        return api;
    }
}
