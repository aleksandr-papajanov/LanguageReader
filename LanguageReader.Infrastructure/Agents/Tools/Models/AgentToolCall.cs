namespace LanguageReader.Infrastructure.Agents.Tools.Models;

/// <summary>
/// Tool call requested by an AI provider.
/// </summary>
public sealed record AgentToolCall(
    string Id,
    string Name,
    string ArgumentsJson);

