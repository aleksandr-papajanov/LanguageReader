using LanguageReader.Infrastructure.Ai.Models;

namespace LanguageReader.Infrastructure.Ai.Execution;

public interface IAiConversationOperation<TState, TResult>
    : IAiOperation<IAiConversationSession<TState, TResult>>
{
    TState InitialState { get; }

    AiJsonOperationRequest BuildRequest<TPayload>(
        TState state,
        IAiConversationStep<TPayload> step);

    AiConversationTurn<TState, TPayload> ApplyTurn<TPayload>(
        TState state,
        IAiConversationStep<TPayload> step,
        AiOperationExecutionResult<TPayload> result);

    TResult Complete(TState state);
}
