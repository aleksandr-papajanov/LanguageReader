using LanguageReader.Infrastructure.Ai.Execution;
using LanguageReader.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LanguageReader.Infrastructure.Ai.Workflows;

public sealed class WorkflowRunner(
    IServiceProvider services,
    IAiExecutor executor,
    ApplicationDbContext repository,
    ILogger<WorkflowRunner> logger)
{
    public Task<TResult> RunAsync<TWorkflow, TRequest, TResult>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TWorkflow : IWorkflow<TRequest, TResult>
    {
        var workflow = services.GetRequiredService<TWorkflow>();
        var context = new WorkflowContext(executor, repository, services, logger);

        return workflow.RunAsync(request, context, cancellationToken);
    }
}
