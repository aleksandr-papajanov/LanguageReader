namespace LanguageReader.Infrastructure.Ai.Providers.Models;

/// <summary>
/// Provider-neutral AI request.
/// </summary>
public sealed record AiProviderRequest(
    IReadOnlyList<AiProviderChatMessage> Messages,
    IReadOnlyList<AiProviderToolDefinition> Tools,
    IReadOnlyList<AiProviderToolResult> ToolResults,
    AiProviderResponseFormat ResponseFormat,
    string? SchemaName,
    string? JsonSchema,
    string? Model,
    int? MaxOutputTokens = null,
    string? PreviousResponseId = null);
