using LanguageReader.Infrastructure.Agents.Providers.Models;

namespace LanguageReader.Infrastructure.Features.Ai.Models;

internal static class AiOperationUsageFactory
{
    public static AiOperationUsageDto Create(
        AiOperationKind kind,
        string provider,
        string model,
        string input,
        string output,
        decimal inputCostPerMillionTokensUsd,
        decimal outputCostPerMillionTokensUsd,
        AiProviderUsage? providerUsage = null)
    {
        return providerUsage.ToAiOperationUsageDto(
            kind,
            provider,
            model,
            input,
            output,
            inputCostPerMillionTokensUsd,
            outputCostPerMillionTokensUsd);
    }
}
