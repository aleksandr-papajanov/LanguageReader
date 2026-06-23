namespace LanguageReader.Api.Features.ReadingItemTranslations;

internal static class ReadingItemTranslationsEndpoints
{
    public static IEndpointRouteBuilder MapReadingItemTranslationsEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapGetReadingItemTranslationsEndpoint();
        api.MapCreateReadingItemTranslationEndpoint();
        api.MapUpdateReadingItemTranslationDisplayEndpoint();
        api.MapDeleteReadingItemTranslationEndpoint();

        return api;
    }
}

