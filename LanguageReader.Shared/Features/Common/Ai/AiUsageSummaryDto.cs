namespace LanguageReader.Shared.Features.Common;

public sealed record AiUsageSummaryDto(
    int OperationCount,
    int InputTokens,
    int OutputTokens,
    int TotalTokens,
    decimal TotalCostUsd);
