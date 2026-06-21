namespace LanguageReader.Infrastructure.Agents.Core.Models;

/// <summary>
/// Message exchanged during an agent run.
/// </summary>
public sealed record AgentMessage(
    string Role,
    string Content);

