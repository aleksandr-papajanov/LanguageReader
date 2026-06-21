namespace LanguageReader.Infrastructure.Agents.Json.Operations;

public interface IAiJsonOperationRunner
{
    Task<AiJsonOperationExecutionResult<TPayload>> RunAsync<TPayload>(
        IAiJsonOperation<TPayload> operation,
        CancellationToken cancellationToken = default);
}
