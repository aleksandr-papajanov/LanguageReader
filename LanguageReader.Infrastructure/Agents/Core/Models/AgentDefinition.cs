using LanguageReader.Infrastructure.Agents.Tools.Models;

namespace LanguageReader.Infrastructure.Agents.Core.Models;

/// <summary>
/// Defines a reusable agent configuration.
/// </summary>
public sealed record AgentDefinition(
    string Name,
    IReadOnlyList<AgentMessage> Messages,
    AgentResponseFormat ResponseFormat,
    IReadOnlyList<AgentToolDefinition> Tools,
    int MaxToolIterations = 4,
    string? Model = null,
    string? SchemaName = null,
    string? JsonSchema = null,
    bool CompleteOnToolResult = false);