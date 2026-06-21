
namespace LanguageReader.Shared.Features.Settings;

public sealed record UserSettingsDto(
    string Username,
    string? NativeLanguage,
    AiServiceMode AiServiceMode);

