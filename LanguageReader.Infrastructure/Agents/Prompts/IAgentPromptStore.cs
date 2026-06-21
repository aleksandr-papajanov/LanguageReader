using LanguageReader.Infrastructure.Agents.Core.Models;

namespace LanguageReader.Infrastructure.Agents.Prompts;

/// <summary>
/// Stores and retrieves agent prompt templates.
/// </summary>
public interface IAgentPromptStore
{
    /// <summary>
    /// Gets a prompt by key.
    /// </summary>
    Task<AgentPrompt?> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or replaces a prompt.
    /// </summary>
    Task SaveAsync(AgentPrompt prompt, CancellationToken cancellationToken = default);
}

