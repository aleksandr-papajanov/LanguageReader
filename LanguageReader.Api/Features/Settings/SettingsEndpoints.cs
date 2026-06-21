namespace LanguageReader.Api.Features.Settings;

internal static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapGetUserSettingsEndpoint();
        api.MapUpdateUserSettingsEndpoint();

        return api;
    }
}

