namespace LanguageReader.Api.Features.Translation;

internal static class TranslationEndpoints
{
    public static IEndpointRouteBuilder MapTranslationEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapTranslateSelectionEndpoint();
        return api;
    }
}

