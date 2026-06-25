using LanguageReader.Infrastructure.Ai.Models;

namespace LanguageReader.Infrastructure.Ai.Execution;

public interface IAiJsonOperation<TPayload> : IAiOperation<AiOperationExecutionResult<TPayload>>
{
    string OperationName { get; }

    AiJsonOperationRequest BuildRequest();
}
