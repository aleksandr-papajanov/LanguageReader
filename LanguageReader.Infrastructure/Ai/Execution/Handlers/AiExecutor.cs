using LanguageReader.Infrastructure.Exceptions;

namespace LanguageReader.Infrastructure.Ai.Execution;

public sealed class AiExecutor(
    IEnumerable<IAiExecutionHandler> handlers) : IAiExecutor
{
    private readonly IReadOnlyList<IAiExecutionHandler> handlers = handlers.ToArray();

    public Task<TResult> ExecuteAsync<TResult>(
        IAiOperation<TResult> operation,
        CancellationToken cancellationToken = default)
    {
        var handler = handlers.FirstOrDefault(item => item.CanHandle(operation));
        if (handler is null)
        {
            throw new InfrastructureException(
                $"No AI execution handler is registered for '{operation.GetType().Name}'.");
        }

        return handler.ExecuteAsync(operation, cancellationToken);
    }
}
