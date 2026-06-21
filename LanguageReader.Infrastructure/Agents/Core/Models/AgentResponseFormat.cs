namespace LanguageReader.Infrastructure.Agents.Core.Models;

/// <summary>
/// Expected response shape from an agent run.
/// </summary>
public enum AgentResponseFormat
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

