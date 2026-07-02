using LanguageReader.Infrastructure.Features.Settings.Services;

namespace LanguageReader.Api.Features.Settings;

internal sealed class GetUserSettingsHandler(UserSettingsService userSettings)
{
    public async Task<UserSettingsDto> HandleAsync(GetUserSettingsRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var settings = await userSettings.GetOrCreateAsync(normalizedUsername, ct);
        return settings.ToUserSettingsDto();
    }
}
