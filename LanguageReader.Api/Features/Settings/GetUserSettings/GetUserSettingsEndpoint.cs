namespace LanguageReader.Api.Features.Settings;

internal static class GetUserSettingsEndpoint
{
    public static IEndpointRouteBuilder MapGetUserSettingsEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapGet("/settings/{username}", async (
            [AsParameters] GetUserSettingsRequest request,
            GetUserSettingsHandler handler,
            CancellationToken ct) => Results.Ok(await handler.HandleAsync(request, ct)))
        .WithName("GetUserSettings")
        .WithOpenApi();

        return api;
    }
}
