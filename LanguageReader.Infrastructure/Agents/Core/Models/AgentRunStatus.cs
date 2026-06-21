namespace LanguageReader.Infrastructure.Agents.Core.Models;

/// <summary>
/// Status of an agent run.
/// </summary>
public enum AgentRunStatus
{
    /// <summary>
    /// The run completed with a final response.
    /// </summary>
    Completed = 0,

    /// <summary>
    /// The run stopped because it reached the configured loop limit.
    /// </summary>
    MaxIterationsReached = 1,

    /// <summary>
    /// The provider returned a failed response.
    /// </summary>
    Failed = 2
}

