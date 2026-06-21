using LanguageReader.Infrastructure.Agents.Json.Models;
using LanguageReader.Infrastructure.Features.Ai.Models;

namespace LanguageReader.Infrastructure.Agents.Json.Operations;

public interface IAiJsonOperation<TPayload>
{
    AiOperationKind Kind { get; }

    string ProviderName { get; }

    AiJsonOperationRequest BuildRequest();
}
