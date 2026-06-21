namespace LanguageReader.Shared.Configuration;

/// <summary>
/// Browser client origins allowed to call the API.
/// </summary>
public sealed class ClientCorsOptions
{
    /// <summary>
    /// Configuration section name for CORS settings.
    /// </summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// Exact frontend origins allowed by the API.
    /// </summary>
    public string[] AllowedOrigins { get; init; } = [];
}
