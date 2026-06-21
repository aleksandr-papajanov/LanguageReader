namespace LanguageReader.Api.Features.Vocabulary;

internal static class DeleteVocabularyEntryEndpoint
{
    public static IEndpointRouteBuilder MapDeleteVocabularyEntryEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapDelete("/api/vocabulary/{VocabularyId:guid}",
            async ([AsParameters] DeleteVocabularyEntryRequest request,
                   DeleteVocabularyEntryHandler handler,
                   CancellationToken ct) =>
            {
                await handler.HandleAsync(request, ct);
                return Results.NoContent();
            })
        .WithName("DeleteVocabularyEntry")
        .WithTags("Vocabulary");

        return api;
    }
}
