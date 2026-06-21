namespace LanguageReader.Api.Features.Vocabulary;

internal static class GetVocabularyEntryEndpoint
{
    public static IEndpointRouteBuilder MapGetVocabularyEntryEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapGet("/vocabulary/{vocabularyId:guid}", async (
            [AsParameters] GetVocabularyEntryRequest request,
            GetVocabularyEntryHandler handler,
            CancellationToken ct) => Results.Ok(await handler.HandleAsync(request, ct)))
        .WithName("GetVocabularyEntry")
        .WithOpenApi();

        return api;
    }
}
