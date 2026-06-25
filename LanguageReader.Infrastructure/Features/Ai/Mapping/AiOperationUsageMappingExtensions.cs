using LanguageReader.Infrastructure.Ai.Providers.Models;

namespace LanguageReader.Infrastructure.Features.Ai.Models;

internal static class AiOperationUsageMappingExtensions
{
    public static AiOperationUsageDto ToAiOperationUsageDto(
        this AiProviderUsage? providerUsage,
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
        decimal outputCostPerMillionTokensUsd)
    {
        var normalizedOperationName = string.IsNullOrWhiteSpace(operationName) ? "Unknown operation" : operationName.Trim();
        var normalizedProvider = string.IsNullOrWhiteSpace(provider) ? "Unknown" : provider.Trim();
        var normalizedModel = string.IsNullOrWhiteSpace(model) ? "unknown" : model.Trim();
        var normalizedExecutionMode = string.IsNullOrWhiteSpace(executionMode) ? "Unknown" : executionMode.Trim();
        var normalizedToolNames = string.IsNullOrWhiteSpace(toolNames) ? null : toolNames.Trim();
        var normalizedTurnCount = Math.Max(1, turnCount);
        var normalizedToolCallCount = Math.Max(0, toolCallCount);
        var estimatedInputTokens = EstimateTokens(input);
        var estimatedOutputTokens = EstimateTokens(output);
        var inputTokens = providerUsage?.InputTokens ?? estimatedInputTokens;
        var outputTokens = providerUsage?.OutputTokens ?? estimatedOutputTokens;
        var totalTokens = providerUsage?.TotalTokens ?? (inputTokens + outputTokens);

        var inputCost = RoundUsd(inputTokens / 1_000_000m * inputCostPerMillionTokensUsd);
        var outputCost = RoundUsd(outputTokens / 1_000_000m * outputCostPerMillionTokensUsd);
        var totalCost = RoundUsd(inputCost + outputCost);

        return new AiOperationUsageDto(
            normalizedOperationName,
            normalizedProvider,
            normalizedModel,
            normalizedExecutionMode,
            normalizedTurnCount,
            normalizedToolCallCount,
            normalizedToolNames,
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
            second.OperationName,
            second.Provider,
            second.Model,
            second.ExecutionMode,
            first.TurnCount + second.TurnCount,
            first.ToolCallCount + second.ToolCallCount,
            MergeToolNames(first.ToolNames, second.ToolNames),
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

    private static string? MergeToolNames(string? first, string? second)
    {
        var names = new[] { first, second }
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .SelectMany(item => item!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return names.Length == 0 ? null : string.Join(", ", names);
    }
}
