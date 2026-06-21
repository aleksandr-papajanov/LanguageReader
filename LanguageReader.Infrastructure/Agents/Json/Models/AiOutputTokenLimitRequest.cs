namespace LanguageReader.Infrastructure.Agents.Json.Models;

/// <summary>
/// Input for calculating a reliable max output token budget.
/// </summary>
public sealed record AiOutputTokenLimitRequest(
    AiOperationKind Kind,
    int SourceTextLength,
    int ContextTextLength,
    int InputJsonLength,
    int ExpectedJsonPropertyCount);
