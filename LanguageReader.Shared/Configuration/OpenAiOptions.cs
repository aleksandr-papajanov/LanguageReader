namespace LanguageReader.Shared.Configuration;

/// <summary>
/// OpenAI-compatible AI provider configuration bound from the OpenAI configuration section.
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
    /// Provider name stored in AI usage metadata.
    /// </summary>
    public string ProviderName { get; init; } = "OpenAI";

    /// <summary>
    /// Base API URL for OpenAI-compatible AI requests.
    /// </summary>
    public string BaseUrl { get; init; } = "https://api.openai.com/v1/";

    /// <summary>
    /// Default model identifier for AI features.
    /// </summary>
    public string? DefaultModel { get; init; }
}

