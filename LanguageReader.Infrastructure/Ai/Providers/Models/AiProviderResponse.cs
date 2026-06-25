namespace LanguageReader.Infrastructure.Ai.Providers.Models;

/// <summary>
/// Provider-neutral AI response.
/// </summary>
public sealed record AiProviderResponse(
    string? ResponseId,
    string? Text,
    string? StructuredJson,
    IReadOnlyList<AiProviderToolCall> ToolCalls,
    bool IsSuccess,
    string? Error = null,
    AiProviderUsage? Usage = null,
    string? IncompleteReason = null);
