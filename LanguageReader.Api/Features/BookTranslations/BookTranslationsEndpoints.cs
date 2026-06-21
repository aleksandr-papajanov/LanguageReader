namespace LanguageReader.Api.Features.BookTranslations;

internal static class BookTranslationsEndpoints
{
    public static IEndpointRouteBuilder MapBookTranslationsEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapGetBookTranslationsEndpoint();
        api.MapCreateBookTranslationEndpoint();
        api.MapUpdateBookTranslationDisplayEndpoint();
        api.MapDeleteBookTranslationEndpoint();

        return api;
    }
}

