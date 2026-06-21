namespace LanguageReader.Api.Features.Vocabulary;

internal static class VocabularyEndpoints
{
    public static IEndpointRouteBuilder MapVocabularyEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapGetVocabularyEndpoint();
        api.MapGetVocabularyEntryEndpoint();
        api.MapSaveVocabularyEntryEndpoint();
        api.MapDeleteVocabularyEntryEndpoint();
        api.MapAutofillVocabularyEntryEndpoint();
        api.MapAddVocabularyExampleEndpoint();
        api.MapDeleteVocabularyExampleEndpoint();
        api.MapUpdateVocabularyVisibilityEndpoint();

        return api;
    }
}

