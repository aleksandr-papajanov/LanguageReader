using LanguageReader.Infrastructure.Ai.Models;

namespace LanguageReader.Infrastructure.Ai.Execution;

public interface IAiConversationStep<TPayload>
{
    string OperationName { get; }
}
