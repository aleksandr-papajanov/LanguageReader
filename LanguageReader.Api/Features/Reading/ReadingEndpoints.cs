namespace LanguageReader.Api.Features.Reading;

internal static class ReadingEndpoints
{
    public static IEndpointRouteBuilder MapReadingEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapGetReadingProgressEndpoint();
        api.MapSaveReadingProgressEndpoint();

        return api;
    }
}

