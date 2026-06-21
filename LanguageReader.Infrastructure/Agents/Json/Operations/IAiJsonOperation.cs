using LanguageReader.Infrastructure.Agents.Json.Models;

namespace LanguageReader.Infrastructure.Agents.Json.Operations;

public interface IAiJsonOperation<TPayload>
{
    AiOperationKind Kind { get; }

    string ProviderName { get; }

    AiJsonOperationRequest BuildRequest();
}
