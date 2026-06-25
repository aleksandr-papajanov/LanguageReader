namespace LanguageReader.Infrastructure.Ai.Execution;

public interface IAiExecutionHandler
{
    bool CanHandle<TResult>(IAiOperation<TResult> operation);

    Task<TResult> ExecuteAsync<TResult>(
        IAiOperation<TResult> operation,
        CancellationToken cancellationToken = default);
}
