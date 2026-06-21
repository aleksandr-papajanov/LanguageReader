namespace LanguageReader.Api.Features.Vocabulary;

internal static class DeleteVocabularyExampleEndpoint
{
    public static IEndpointRouteBuilder MapDeleteVocabularyExampleEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapDelete("/vocabulary/{vocabularyId:guid}/examples/{exampleId:guid}", async (
            [AsParameters] DeleteVocabularyExampleRequest request,
            DeleteVocabularyExampleHandler handler,
            CancellationToken ct) =>
        {
            return Results.Ok(await handler.HandleAsync(request, ct));
        })
        .WithName("DeleteVocabularyExample")
        .WithOpenApi();

        return api;
    }
}
