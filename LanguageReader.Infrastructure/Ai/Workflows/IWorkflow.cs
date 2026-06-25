namespace LanguageReader.Infrastructure.Ai.Workflows;

public interface IWorkflow<TRequest, TResult>
{
    Task<TResult> RunAsync(
        TRequest request,
        WorkflowContext context,
        CancellationToken cancellationToken = default);
}
