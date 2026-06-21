namespace LanguageReader.Infrastructure.Agents.Json;

public interface IAiModelResolver
{
    string Resolve(string? configuredModel);
}
