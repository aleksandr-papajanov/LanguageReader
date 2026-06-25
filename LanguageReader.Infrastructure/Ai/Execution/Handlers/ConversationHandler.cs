using LanguageReader.Infrastructure.Ai.Models;
using LanguageReader.Infrastructure.Features.Ai.Models;
using LanguageReader.Shared.Configuration;
using Microsoft.Extensions.Options;

namespace LanguageReader.Infrastructure.Ai.Execution;

public sealed class ConversationHandler(
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
                && type.GetGenericTypeDefinition() == typeof(IAiConversationOperation<,>));
    }

    public Task<TResult> ExecuteAsync<TResult>(
        IAiOperation<TResult> operation,
        CancellationToken cancellationToken = default)
    {
        var session = CreateSession((dynamic)operation);
        return Task.FromResult((TResult)session);
    }

    private AiConversationSession<TState, TResult> CreateSession<TState, TResult>(
        IAiConversationOperation<TState, TResult> operation)
    {
        return new AiConversationSession<TState, TResult>(
            operation,
            jsonRequestService,
            ProviderName);
    }

    private sealed class AiConversationSession<TState, TResult>(
        IAiConversationOperation<TState, TResult> operation,
        IAiJsonRequestService jsonRequestService,
        string providerName)
        : IAiConversationSession<TState, TResult>
    {
        public TState State { get; private set; } = operation.InitialState;

        public async Task<AiConversationTurn<TState, TPayload>> SendAsync<TPayload>(
            IAiConversationStep<TPayload> step,
            CancellationToken cancellationToken = default)
        {
            var request = operation.BuildRequest(State, step);
            var result = await jsonRequestService.CompleteAsync<TPayload>(request, cancellationToken);
            var pricing = AiPricingCatalog.GetPricing(providerName, result.Model);

            var executionResult = new AiOperationExecutionResult<TPayload>(
                result.Payload,
                result.Model,
                result.RawJson,
                AiOperationUsageFactory.Create(
                    step.OperationName,
                    providerName,
                    result.Model,
                    executionMode: "Conversation",
                    turnCount: 1,
                    toolCallCount: 0,
                    toolNames: null,
                    BuildPromptPreview(request.Messages),
                    result.RawJson,
                    pricing.InputUsdPerMillionTokens,
                    pricing.OutputUsdPerMillionTokens,
                    result.Usage),
                result.ResponseId,
                result.IncompleteReason);

            var turn = operation.ApplyTurn(State, step, executionResult);
            State = turn.State;

            return turn;
        }

        public Task<TResult> CompleteAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(operation.Complete(State));
        }

        private static string BuildPromptPreview(IReadOnlyList<AiProviderMessage> messages)
        {
            return string.Join(
                Environment.NewLine + Environment.NewLine,
                messages.Select(message => $"{message.Role}: {message.Content}"));
        }
    }
}
