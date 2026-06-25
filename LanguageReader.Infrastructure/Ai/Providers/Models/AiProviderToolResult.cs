namespace LanguageReader.Infrastructure.Ai.Providers.Models;

/// <summary>
/// Result returned after executing a tool call.
/// </summary>
public sealed record AiProviderToolResult(
    string ToolCallId,
    string OutputJson,
    bool IsError = false,
    string? ToolName = null);

