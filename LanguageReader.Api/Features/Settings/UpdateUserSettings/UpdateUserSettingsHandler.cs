using LanguageReader.Infrastructure.Features.Settings.Services;

namespace LanguageReader.Api.Features.Settings;

internal sealed class UpdateUserSettingsHandler(UserSettingsService userSettings)
{
    public async Task<UserSettingsDto> HandleAsync(UpdateUserSettingsRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var settings = await userSettings.UpdateNativeLanguageAsync(
            normalizedUsername,
            request.NativeLanguage,
            ct);

        return settings.ToUserSettingsDto();
    }
}
