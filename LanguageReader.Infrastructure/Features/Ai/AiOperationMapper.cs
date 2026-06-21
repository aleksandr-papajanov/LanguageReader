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
            Kind = usage.Kind,
            Provider = usage.Provider,
            Model = usage.Model,
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
