using LanguageReader.Infrastructure.Agents.Tools.Models;
using System.Text.Json;

namespace LanguageReader.Infrastructure.Agents.Tools;

/// <summary>
/// Default DI-backed tool dispatcher.
/// </summary>
public sealed class ToolDispatcher(IEnumerable<IAgentTool> tools) : IToolDispatcher
{
    private readonly Dictionary<string, IAgentTool> toolsByName = tools.ToDictionary(
        tool => tool.Definition.Name,
        StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<AgentToolResult> DispatchAsync(AgentToolCall call, CancellationToken cancellationToken = default)
    {
        if (!toolsByName.TryGetValue(call.Name, out var tool))
        {
            return new AgentToolResult(
                call.Id,
                JsonSerializer.Serialize(new { error = $"Tool '{call.Name}' is not registered." }),
                IsError: true,
                ToolName: call.Name);
        }

        try
        {
            var result = await tool.ExecuteAsync(call, cancellationToken);
            return result.ToolName is null ? result with { ToolName = call.Name } : result;
        }
        catch (Exception exception)
        {
            return new AgentToolResult(
                call.Id,
                JsonSerializer.Serialize(new { error = exception.Message }),
                IsError: true,
                ToolName: call.Name);
        }
    }
}

