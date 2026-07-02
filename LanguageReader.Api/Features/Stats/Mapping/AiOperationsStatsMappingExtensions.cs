using LanguageReader.Api.Features.Common.Mapping;
using LanguageReader.Infrastructure.Features.Ai.Entities;

namespace LanguageReader.Api.Features.Stats;

internal static class AiOperationsStatsMappingExtensions
{
    public static GetAiOperationsStatsResponse ToGetAiOperationsStatsResponse(
        this IReadOnlyList<AiOperationEntity> operations)
    {
        return new GetAiOperationsStatsResponse(
            operations.ToAiUsageSummaryDto(),
            operations.Select(operation => operation.ToAiOperationDto()).ToList());
    }
}
