using LanguageReader.Infrastructure.Ai.Execution;
using LanguageReader.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace LanguageReader.Infrastructure.Ai.Workflows;

public sealed class WorkflowContext(
    IAiExecutor executor,
    ApplicationDbContext repository,
    IServiceProvider services,
    ILogger logger)
{
    public IServiceProvider Services { get; } = services;

    public ApplicationDbContext Repository { get; } = repository;

    public ILogger Logger { get; } = logger;

    public Task<TResult> ExecuteAsync<TResult>(
        IAiOperation<TResult> operation,
        CancellationToken cancellationToken = default)
    {
        return executor.ExecuteAsync(operation, cancellationToken);
    }
}
