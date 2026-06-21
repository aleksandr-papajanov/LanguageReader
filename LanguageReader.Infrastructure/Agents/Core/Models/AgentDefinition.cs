using LanguageReader.Infrastructure.Agents.Tools.Models;

namespace LanguageReader.Infrastructure.Agents.Core.Models;

/// <summary>
/// Defines a reusable agent configuration.
/// </summary>
public sealed record AgentDefinition(
    string Name,
    string Instructions,
    AgentResponseFormat ResponseFormat,
    IReadOnlyList<AgentToolDefinition> Tools,
    int MaxToolIterations = 4,
    string? Model = null,
    bool CompleteOnToolResult = false);

