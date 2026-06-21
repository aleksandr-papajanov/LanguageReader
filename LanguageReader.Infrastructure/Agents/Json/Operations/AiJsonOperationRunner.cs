using LanguageReader.Infrastructure.Features.Ai.Models;

namespace LanguageReader.Infrastructure.Agents.Json.Operations;

public sealed class AiJsonOperationRunner(
    IAiJsonRequestService jsonRequestService) : IAiJsonOperationRunner
{
    public async Task<AiJsonOperationExecutionResult<TPayload>> RunAsync<TPayload>(
        IAiJsonOperation<TPayload> operation,
        CancellationToken cancellationToken = default)
    {
        var request = operation.BuildRequest();
        var result = await jsonRequestService.CompleteAsync<TPayload>(request, cancellationToken);
        var pricing = AiPricingCatalog.GetPricing(operation.ProviderName, result.Model);
        var usage = AiOperationUsageFactory.Create(
            operation.Kind,
            operation.ProviderName,
            result.Model,
            request.InputJson,
            result.RawJson,
            pricing.InputUsdPerMillionTokens,
            pricing.OutputUsdPerMillionTokens,
            result.Usage);

        return new AiJsonOperationExecutionResult<TPayload>(
            result.Payload,
            result.Model,
            result.RawJson,
            usage,
            result.ResponseId,
            result.IncompleteReason);
    }
}
