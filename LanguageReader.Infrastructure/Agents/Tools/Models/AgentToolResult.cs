namespace LanguageReader.Infrastructure.Agents.Tools.Models;

/// <summary>
/// Result returned after executing a tool call.
/// </summary>
public sealed record AgentToolResult(
    string ToolCallId,
    string OutputJson,
    bool IsError = false,
    string? ToolName = null);

