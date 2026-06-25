using LanguageReader.Infrastructure.Ai.Models;

namespace LanguageReader.Infrastructure.Ai.Execution;

public interface IAiJsonRequestService
{
    Task<AiJsonOperationResult<TPayload>> CompleteAsync<TPayload>(
        AiJsonOperationRequest request,
        CancellationToken cancellationToken = default);
}
