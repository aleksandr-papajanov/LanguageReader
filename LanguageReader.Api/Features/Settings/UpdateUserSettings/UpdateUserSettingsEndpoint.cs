namespace LanguageReader.Api.Features.Settings;

internal static class UpdateUserSettingsEndpoint
{
    public static IEndpointRouteBuilder MapUpdateUserSettingsEndpoint(this IEndpointRouteBuilder api)
    {
        api.MapPut("/settings/{username}", async (
            [AsParameters] UpdateUserSettingsRequestRoute route,
            UpdateUserSettingsRequestBody body,
            UpdateUserSettingsHandler handler,
            CancellationToken ct) =>
        {
            var request = new UpdateUserSettingsRequest(
                route.Username,
                body.NativeLanguage,
                body.AiServiceMode);

            return Results.Ok(await handler.HandleAsync(request, ct));
        })
        .WithName("UpdateUserSettings")
        .WithOpenApi();

        return api;
    }
}
