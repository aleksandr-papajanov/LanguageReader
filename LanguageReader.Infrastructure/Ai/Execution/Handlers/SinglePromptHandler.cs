using LanguageReader.Infrastructure.Ai.Models;
using LanguageReader.Infrastructure.Features.Ai.Models;
using LanguageReader.Shared.Configuration;
using Microsoft.Extensions.Options;

namespace LanguageReader.Infrastructure.Ai.Execution;

public sealed class SinglePromptHandler(
    IAiJsonRequestService jsonRequestService,
    IOptions<OpenAiOptions> options) : IAiExecutionHandler
{
    private string ProviderName => string.IsNullOrWhiteSpace(options.Value.ProviderName)
        ? "Unknown"
        : options.Value.ProviderName.Trim();


    public bool CanHandle<TResult>(IAiOperation<TResult> operation)
    {
        return operation.GetType()
            .GetInterfaces()
            .Any(type => type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(IAiJsonOperation<>));
    }

    public async Task<TResult> ExecuteAsync<TResult>(
        IAiOperation<TResult> operation,
        CancellationToken cancellationToken = default)
    {
        var result = await ExecuteJsonAsync((dynamic)operation, cancellationToken);
        return (TResult)result;
    }

    private async Task<AiOperationExecutionResult<TPayload>> ExecuteJsonAsync<TPayload>(
        IAiJsonOperation<TPayload> operation,
        CancellationToken cancellationToken)
    {
        var request = operation.BuildRequest();
        var result = await jsonRequestService.CompleteAsync<TPayload>(request, cancellationToken);
        var providerName = ProviderName;
        var pricing = AiPricingCatalog.GetPricing(providerName, result.Model);

        var usage = AiOperationUsageFactory.Create(
            operation.OperationName,
            providerName,
            result.Model,
            executionMode: "SinglePrompt",
            turnCount: 1,
            toolCallCount: 0,
            toolNames: null,
            BuildPromptPreview(request.Messages),
            result.RawJson,
            pricing.InputUsdPerMillionTokens,
            pricing.OutputUsdPerMillionTokens,
            result.Usage);

        return new AiOperationExecutionResult<TPayload>(
            result.Payload,
            result.Model,
            result.RawJson,
            usage,
            result.ResponseId,
            result.IncompleteReason);
    }

    private static string BuildPromptPreview(IReadOnlyList<AiProviderMessage> messages)
    {
        return string.Join(
            Environment.NewLine + Environment.NewLine,
            messages.Select(message => $"{message.Role}: {message.Content}"));
    }
}
