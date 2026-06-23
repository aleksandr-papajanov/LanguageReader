namespace LanguageReader.Infrastructure.Agents.Core.Models;

/// <summary>
/// Message exchanged during an agent run.
/// </summary>
public sealed record AgentMessage(
    AgentMessageRole Role,
    string Content);
