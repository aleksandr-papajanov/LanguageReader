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

}
