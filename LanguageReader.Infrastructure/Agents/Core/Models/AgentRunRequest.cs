namespace LanguageReader.Infrastructure.Agents.Core.Models;

/// <summary>
/// Request to run an agent.
/// </summary>
public sealed record AgentRunRequest(
    string Input,
    IReadOnlyList<AgentMessage>? Messages = null,
    string? CorrelationId = null);

