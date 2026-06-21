using LanguageReader.Infrastructure.Agents.Providers.Models;

namespace LanguageReader.Infrastructure.Features.Ai.Models;

internal static class AiOperationUsageMappingExtensions
{
    public static AiOperationUsageDto ToAiOperationUsageDto(
        this AiProviderUsage? providerUsage,
        AiOperationKind kind,
        string provider,
        string model,
        string input,
        string output,
        decimal inputCostPerMillionTokensUsd,
        decimal outputCostPerMillionTokensUsd)
    {
        var normalizedProvider = string.IsNullOrWhiteSpace(provider) ? "FakeAI" : provider.Trim();
        var normalizedModel = string.IsNullOrWhiteSpace(model) ? "fake-language-v1" : model.Trim();
        var estimatedInputTokens = EstimateTokens(input);
        var estimatedOutputTokens = EstimateTokens(output);
        var inputTokens = providerUsage?.InputTokens ?? estimatedInputTokens;
        var outputTokens = providerUsage?.OutputTokens ?? estimatedOutputTokens;
        var totalTokens = providerUsage?.TotalTokens ?? (inputTokens + outputTokens);

        var inputCost = RoundUsd(inputTokens / 1_000_000m * inputCostPerMillionTokensUsd);
        var outputCost = RoundUsd(outputTokens / 1_000_000m * outputCostPerMillionTokensUsd);
        var totalCost = RoundUsd(inputCost + outputCost);

        return new AiOperationUsageDto(
            kind,
            normalizedProvider,
            normalizedModel,
            inputTokens,
            outputTokens,
            totalTokens,
            inputCost,
            outputCost,
            totalCost);
    }

    public static AiOperationUsageDto MergeWith(this AiOperationUsageDto? first, AiOperationUsageDto second)
    {
        if (first is null)
        {
            return second;
        }

        return new AiOperationUsageDto(
            AiOperationKind.Translation,
            second.Provider,
            second.Model,
            first.InputTokens + second.InputTokens,
            first.OutputTokens + second.OutputTokens,
            first.TotalTokens + second.TotalTokens,
            first.InputCostUsd + second.InputCostUsd,
            first.OutputCostUsd + second.OutputCostUsd,
            first.TotalCostUsd + second.TotalCostUsd);
    }

    private static int EstimateTokens(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        var normalizedLength = text.Trim().Length;
        return Math.Max(1, (int)Math.Ceiling(normalizedLength / 4d));
    }

    private static decimal RoundUsd(decimal value)
    {
        return Math.Round(value, 8, MidpointRounding.AwayFromZero);
    }
}
