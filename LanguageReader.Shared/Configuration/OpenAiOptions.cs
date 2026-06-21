namespace LanguageReader.Shared.Configuration;

/// <summary>
/// OpenAI configuration bound from the OpenAI configuration section.
/// </summary>
public sealed class OpenAiOptions
{
    /// <summary>
    /// Configuration section name for OpenAI settings.
    /// </summary>
    public const string SectionName = "OpenAI";

    /// <summary>
    /// API key supplied through environment variables or user secrets.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// Base API URL for future OpenAI requests.
    /// </summary>
    public string BaseUrl { get; init; } = "https://api.openai.com/v1/";

    /// <summary>
    /// Default model identifier for future AI features.
    /// </summary>
    public string? DefaultModel { get; init; }
}

