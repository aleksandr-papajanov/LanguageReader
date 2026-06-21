namespace LanguageReader.Infrastructure.Agents.Json.Models;

/// <summary>
/// Shared direct-JSON AI request used by feature services.
/// </summary>
public sealed record AiJsonOperationRequest(
    AiOperationKind Kind,
    string OperationName,
    string Instructions,
    string InputJson,
    string? SchemaName,
    string? JsonSchema,
    string? Model,
    int SourceTextLength,
    int ContextTextLength = 0,
    int ExpectedJsonPropertyCount = 0);
