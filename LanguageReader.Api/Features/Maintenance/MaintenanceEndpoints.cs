namespace LanguageReader.Api.Features.Maintenance;

internal static class MaintenanceEndpoints
{
    public static IEndpointRouteBuilder MapMaintenanceEndpoints(this IEndpointRouteBuilder api)
    {
        api.MapResetImportedReadingContentEndpoint();

        return api;
    }
}
