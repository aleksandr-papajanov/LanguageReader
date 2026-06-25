namespace LanguageReader.Infrastructure.Ai.Execution;

public sealed record AiConversationTurn<TState, TPayload>(
    TState State,
    AiOperationExecutionResult<TPayload> Result,
    bool CanComplete)
{
    public TPayload Payload => Result.Payload;
}
