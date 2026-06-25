using LanguageReader.Infrastructure.Features.Ai.Entities;

namespace LanguageReader.Infrastructure.Features.Ai;

public static class AiOperationMapper
{
    public static AiOperationEntity ToEntity(
        AiOperationUsageDto usage,
        string username,
        Guid? translatedRangeId = null,
        Guid? vocabularyEntryId = null)
    {
        return new AiOperationEntity
        {
            Id = Guid.NewGuid(),
            Username = username,
            OperationName = usage.OperationName,
            Provider = usage.Provider,
            Model = usage.Model,
            ExecutionMode = usage.ExecutionMode,
            TurnCount = usage.TurnCount,
            ToolCallCount = usage.ToolCallCount,
            ToolNames = usage.ToolNames,
            InputTokens = usage.InputTokens,
            OutputTokens = usage.OutputTokens,
            TotalTokens = usage.TotalTokens,
            InputCostUsd = usage.InputCostUsd,
            OutputCostUsd = usage.OutputCostUsd,
            TotalCostUsd = usage.TotalCostUsd,
            TranslatedRangeId = translatedRangeId,
            VocabularyEntryId = vocabularyEntryId,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
