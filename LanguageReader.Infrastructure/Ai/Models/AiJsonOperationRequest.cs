namespace LanguageReader.Infrastructure.Ai.Models;

/// <summary>
/// Shared direct-JSON AI request used by feature operations.
/// </summary>
public sealed record AiJsonOperationRequest(
    string OperationName,
    IReadOnlyList<AiProviderMessage> Messages,
    string? SchemaName,
    string? JsonSchema,
    int SourceTextLength,
    int ContextTextLength = 0,
    int ExpectedJsonPropertyCount = 0);
