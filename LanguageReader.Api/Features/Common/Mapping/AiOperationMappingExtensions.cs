using LanguageReader.Infrastructure.Features.Ai.Entities;

namespace LanguageReader.Api.Features.Common.Mapping;

internal static class AiOperationMappingExtensions
{
    public static AiOperationDto ToAiOperationDto(this AiOperationEntity operation)
    {
        return new AiOperationDto(
            operation.Id,
            operation.OperationName,
            operation.Provider,
            operation.Model,
            operation.ExecutionMode,
            operation.TurnCount,
            operation.ToolCallCount,
            operation.ToolNames,
            operation.InputTokens,
            operation.OutputTokens,
            operation.TotalTokens,
            operation.InputCostUsd,
            operation.OutputCostUsd,
            operation.TotalCostUsd,
            operation.CreatedAtUtc);
    }

    public static AiUsageSummaryDto ToAiUsageSummaryDto(this IEnumerable<AiOperationEntity> operations)
    {
        var items = operations.ToList();

        return new AiUsageSummaryDto(
            items.Count,
            items.Sum(item => item.InputTokens),
            items.Sum(item => item.OutputTokens),
            items.Sum(item => item.TotalTokens),
            items.Sum(item => item.TotalCostUsd));
    }
}
