using LanguageReader.Infrastructure.Agents.Tools.Models;

namespace LanguageReader.Infrastructure.Agents.Tools;

/// <summary>
/// Dispatches provider tool calls to registered application tools.
/// </summary>
public interface IToolDispatcher
{
    /// <summary>
    /// Executes a tool call.
    /// </summary>
    Task<AgentToolResult> DispatchAsync(AgentToolCall call, CancellationToken cancellationToken = default);
}

