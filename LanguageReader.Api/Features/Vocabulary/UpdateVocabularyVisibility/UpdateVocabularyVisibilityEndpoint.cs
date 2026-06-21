namespace LanguageReader.Api.Features.Vocabulary;

internal static class UpdateVocabularyVisibilityEndpoint
{
    public static IEndpointRouteBuilder MapUpdateVocabularyVisibilityEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPut("/vocabulary/{vocabularyId:guid}/visibility", async (
            [AsParameters] UpdateVocabularyVisibilityRequestRoute route,
            UpdateVocabularyVisibilityRequestBody body,
            UpdateVocabularyVisibilityHandler handler,
            CancellationToken ct) =>
        {
            var request = new UpdateVocabularyVisibilityRequest(
                route.VocabularyId,
                body.Username,
                body.IsVisibleInVocabulary);

            return Results.Ok(await handler.HandleAsync(request, ct));
        })
        .WithName("UpdateVocabularyVisibility")
        .WithOpenApi();

        return api;
    }
}


