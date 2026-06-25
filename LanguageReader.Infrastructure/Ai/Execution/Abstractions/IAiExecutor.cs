namespace LanguageReader.Infrastructure.Ai.Execution;

public interface IAiExecutor
{
    Task<TResult> ExecuteAsync<TResult>(
        IAiOperation<TResult> operation,
        CancellationToken cancellationToken = default);
}
