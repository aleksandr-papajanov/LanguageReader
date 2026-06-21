namespace LanguageReader.Api.Features.Translation;

internal static class TranslateSelectionEndpoint
{
    public static IEndpointRouteBuilder MapTranslateSelectionEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPost("/translation", async (
            TranslateRequest request,
            TranslateSelectionHandler handler,
            CancellationToken ct) => Results.Ok(await handler.HandleAsync(request, ct)))
        .WithName("TranslateSelection")
        .WithOpenApi();

        return api;
    }
}


