namespace LanguageReader.Infrastructure.Ai.Execution;

public interface IAiModelResolver
{
    string Resolve(string? requestedModel);
}
