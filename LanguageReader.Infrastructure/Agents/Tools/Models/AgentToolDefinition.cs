namespace LanguageReader.Infrastructure.Agents.Tools.Models;

/// <summary>
/// Describes a tool that a provider can call.
/// </summary>
public sealed record AgentToolDefinition(
    string Name,
    string Description,
    string ParametersJsonSchema);

