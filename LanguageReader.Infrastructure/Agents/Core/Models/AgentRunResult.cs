using LanguageReader.Infrastructure.Agents.Tools.Models;

namespace LanguageReader.Infrastructure.Agents.Core.Models;

/// <summary>
/// Final result from an agent run.
/// </summary>
public sealed record AgentRunResult(
    AgentRunStatus Status,
    string? Text,
    string? StructuredJson,
    IReadOnlyList<AgentToolCall> ToolCalls,
    IReadOnlyList<AgentToolResult> ToolResults,
    string? ProviderResponseId = null,
    string? Error = null);

