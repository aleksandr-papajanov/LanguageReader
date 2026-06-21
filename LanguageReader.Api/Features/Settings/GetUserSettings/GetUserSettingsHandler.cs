namespace LanguageReader.Api.Features.Settings;

internal sealed class GetUserSettingsHandler(UserSettingsAccessor userSettingsAccessor)
{
    public async Task<UserSettingsDto> HandleAsync(GetUserSettingsRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var settings = await userSettingsAccessor.GetOrCreateAsync(normalizedUsername, ct);
        return settings.ToUserSettingsDto();
    }
}
