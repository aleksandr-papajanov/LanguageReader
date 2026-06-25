using LanguageReader.Infrastructure.Ai.Models;
using LanguageReader.Infrastructure.Ai.Providers.Models;

namespace LanguageReader.Infrastructure.Ai.Execution;

public interface IAiAgentOperation<TPayload> : IAiOperation<AiOperationExecutionResult<TPayload>>
{
    string OperationName { get; }

    AiJsonOperationRequest BuildRequest();

    IReadOnlyList<AiProviderToolDefinition> GetTools();

    Task<AiProviderToolResult> ExecuteToolAsync(
        AiProviderToolCall toolCall,
        CancellationToken cancellationToken = default);
}
