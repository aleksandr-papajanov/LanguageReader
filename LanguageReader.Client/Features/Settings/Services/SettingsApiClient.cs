namespace LanguageReader.Client.Features.Settings.Services;

public sealed class SettingsApiClient(ApiClient api)
{
    public Task<UserSettingsDto> GetSettingsAsync(
        GetUserSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.GetAsync<UserSettingsDto>(
            "/api/settings/{Username}",
            request,
            cancellationToken);
    }

    public Task<UserSettingsDto> UpdateSettingsAsync(
        UpdateUserSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        var route = new UpdateUserSettingsRequestRoute(request.Username);
        var body = new UpdateUserSettingsRequestBody(request.NativeLanguage);

        return api.PutAsync<UserSettingsDto>(
            "/api/settings/{Username}",
            route,
            body,
            cancellationToken);
    }
}
