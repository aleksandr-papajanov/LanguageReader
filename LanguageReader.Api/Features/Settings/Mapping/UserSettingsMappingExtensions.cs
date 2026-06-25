using LanguageReader.Infrastructure.Features.Settings.Entities;

namespace LanguageReader.Api.Features.Settings;

internal static class UserSettingsMappingExtensions
{
    public static UserSettingsDto ToUserSettingsDto(this UserSettingsEntity settings)
    {
        return new UserSettingsDto(settings.Username, settings.NativeLanguage);
    }
}
