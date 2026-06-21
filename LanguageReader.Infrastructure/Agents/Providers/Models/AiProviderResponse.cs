using LanguageReader.Infrastructure.Agents.Tools.Models;

namespace LanguageReader.Infrastructure.Agents.Providers.Models;

/// <summary>
/// Provider-neutral AI response.
/// </summary>
public sealed record AiProviderResponse(
    string? ResponseId,
    string? Text,
    string? StructuredJson,
    IReadOnlyList<AgentToolCall> ToolCalls,
    bool IsSuccess,
    string? Error = null,
    AiProviderUsage? Usage = null,
    string? IncompleteReason = null);

