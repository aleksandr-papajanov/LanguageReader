namespace LanguageReader.Infrastructure.Ai.Execution;

public interface IAiConversationSession<TState, TResult>
{
    TState State { get; }

    Task<AiConversationTurn<TState, TPayload>> SendAsync<TPayload>(
        IAiConversationStep<TPayload> step,
        CancellationToken cancellationToken = default);

    Task<TResult> CompleteAsync(CancellationToken cancellationToken = default);
}
