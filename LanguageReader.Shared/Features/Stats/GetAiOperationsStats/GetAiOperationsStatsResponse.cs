namespace LanguageReader.Shared.Features.Stats;

public sealed record GetAiOperationsStatsResponse(
    AiUsageSummaryDto Summary,
    IReadOnlyList<AiOperationDto> Operations);
