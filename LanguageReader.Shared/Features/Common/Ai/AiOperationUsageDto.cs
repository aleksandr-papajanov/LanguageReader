namespace LanguageReader.Shared.Features.Common;

public sealed record AiOperationUsageDto(
    AiOperationKind Kind,
    string Provider,
    string Model,
    int InputTokens,
    int OutputTokens,
    int TotalTokens,
    decimal InputCostUsd,
    decimal OutputCostUsd,
    decimal TotalCostUsd);
