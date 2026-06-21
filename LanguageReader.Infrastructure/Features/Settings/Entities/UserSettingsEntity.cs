using LanguageReader.Shared.Features.Settings;

namespace LanguageReader.Infrastructure.Features.Settings.Entities;

/// <summary>
/// Persisted user learning settings.
/// </summary>
public sealed class UserSettingsEntity
{
    /// <summary>
    /// Temporary username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Selected target learning language.
    /// </summary>
    public string? NativeLanguage { get; set; }

    /// <summary>
    /// Selected AI mode for translation and vocabulary features.
    /// </summary>
    public AiServiceMode AiServiceMode { get; set; } = AiServiceMode.Fake;
}
