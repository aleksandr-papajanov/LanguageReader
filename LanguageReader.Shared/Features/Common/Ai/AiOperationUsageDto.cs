namespace LanguageReader.Shared.Features.Common;

public sealed record AiOperationUsageDto(
    string OperationName,
    string Provider,
    string Model,
    string ExecutionMode,
    int TurnCount,
    int ToolCallCount,
    string? ToolNames,
    int InputTokens,
    int OutputTokens,
    int TotalTokens,
    decimal InputCostUsd,
    decimal OutputCostUsd,
    decimal TotalCostUsd);
