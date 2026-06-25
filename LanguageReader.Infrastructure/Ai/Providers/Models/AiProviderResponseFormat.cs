namespace LanguageReader.Infrastructure.Ai.Providers.Models;

/// <summary>
/// Expected provider response shape.
/// </summary>
public enum AiProviderResponseFormat
{
    /// <summary>
    /// Plain natural-language text.
    /// </summary>
    PlainText = 0,

    /// <summary>
    /// Structured JSON text.
    /// </summary>
    Json = 1
}

