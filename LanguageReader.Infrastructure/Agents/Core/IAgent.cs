using LanguageReader.Infrastructure.Agents.Core.Models;

namespace LanguageReader.Infrastructure.Agents.Core;

/// <summary>
/// Executable agent instance.
/// </summary>
public interface IAgent
{
    /// <summary>
    /// Runs the agent.
    /// </summary>
    Task<AgentRunResult> RunAsync(AgentRunRequest request, CancellationToken cancellationToken = default);
}

