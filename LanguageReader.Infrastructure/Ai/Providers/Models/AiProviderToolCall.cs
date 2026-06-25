namespace LanguageReader.Infrastructure.Ai.Providers.Models;

/// <summary>
/// Tool call requested by an AI provider.
/// </summary>
public sealed record AiProviderToolCall(
    string Id,
    string Name,
    string ArgumentsJson);

