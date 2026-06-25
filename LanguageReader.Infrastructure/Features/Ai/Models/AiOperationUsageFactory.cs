using LanguageReader.Infrastructure.Ai.Providers.Models;

namespace LanguageReader.Infrastructure.Features.Ai.Models;

internal static class AiOperationUsageFactory
{
    public static AiOperationUsageDto Create(
        string operationName,
        string provider,
        string model,
        string executionMode,
        int turnCount,
        int toolCallCount,
        string? toolNames,
        string input,
        string output,
        decimal inputCostPerMillionTokensUsd,
        decimal outputCostPerMillionTokensUsd,
        AiProviderUsage? providerUsage = null)
    {
        return providerUsage.ToAiOperationUsageDto(
            operationName,
            provider,
            model,
            executionMode,
            turnCount,
            toolCallCount,
            toolNames,
            input,
            output,
            inputCostPerMillionTokensUsd,
            outputCostPerMillionTokensUsd);
    }
}
