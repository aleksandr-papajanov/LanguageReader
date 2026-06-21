namespace LanguageReader.Shared.Features.Settings;

public sealed record UpdateUserSettingsRequest(
    string Username,
    string? NativeLanguage,
    AiServiceMode AiServiceMode);

public sealed record UpdateUserSettingsRequestRoute(
    string Username);

public sealed record UpdateUserSettingsRequestBody(
    string? NativeLanguage,
    AiServiceMode AiServiceMode);
