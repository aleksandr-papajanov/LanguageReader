namespace LanguageReader.Api.Features.Vocabulary;

internal static class GetVocabularyEndpoint
{
    public static IEndpointRouteBuilder MapGetVocabularyEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapGet("/vocabulary", async (
            [AsParameters] GetVocabularyRequest request,
            GetVocabularyHandler handler,
            CancellationToken ct) => Results.Ok(await handler.HandleAsync(request, ct)))
        .WithName("GetVocabulary")
        .WithOpenApi();

        return api;
    }
}

