namespace LanguageReader.Infrastructure.Ai.Providers.Models;

/// <summary>
/// Describes a tool that a provider can call.
/// </summary>
public sealed record AiProviderToolDefinition(
    string Name,
    string Description,
    string ParametersJsonSchema);

