namespace LanguageReader.Infrastructure.Agents.Core.Models;

/// <summary>
/// Stored prompt template for agent definitions.
/// </summary>
public sealed record AgentPrompt(
    string Key,
    string Name,
    string Instructions,
    AgentResponseFormat ResponseFormat);

