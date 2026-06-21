using LanguageReader.Infrastructure.Agents.Tools.Models;

namespace LanguageReader.Infrastructure.Agents.Tools;

/// <summary>
/// Tool that can be executed by the agent tool dispatcher.
/// </summary>
public interface IAgentTool
{
    /// <summary>
    /// Tool definition exposed to providers.
    /// </summary>
    AgentToolDefinition Definition { get; }

    /// <summary>
    /// Executes the tool with provider-supplied JSON arguments.
    /// </summary>
    Task<AgentToolResult> ExecuteAsync(AgentToolCall call, CancellationToken cancellationToken = default);
}

