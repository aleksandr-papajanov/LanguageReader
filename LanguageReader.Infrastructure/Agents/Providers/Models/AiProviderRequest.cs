using LanguageReader.Infrastructure.Agents.Core.Models;
using LanguageReader.Infrastructure.Agents.Tools.Models;

namespace LanguageReader.Infrastructure.Agents.Providers.Models;

/// <summary>
/// Provider-neutral AI request.
/// </summary>
public sealed record AiProviderRequest(
    string Instructions,
    IReadOnlyList<AgentMessage> Messages,
    IReadOnlyList<AgentToolDefinition> Tools,
    IReadOnlyList<AgentToolResult> ToolResults,
    AgentResponseFormat ResponseFormat,
    string? SchemaName,
    string? JsonSchema,
    string? Model,
    int? MaxOutputTokens = null);

