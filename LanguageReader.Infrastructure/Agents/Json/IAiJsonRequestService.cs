using LanguageReader.Infrastructure.Agents.Json.Models;

namespace LanguageReader.Infrastructure.Agents.Json;

public interface IAiJsonRequestService
{
    Task<AiJsonOperationResult<TPayload>> CompleteAsync<TPayload>(
        AiJsonOperationRequest request,
        CancellationToken cancellationToken = default);
}
