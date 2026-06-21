using LanguageReader.Infrastructure.Agents.Core.Models;
using System.Collections.Concurrent;

namespace LanguageReader.Infrastructure.Agents.Prompts;

/// <summary>
/// In-memory prompt store used until persisted agent configuration is introduced.
/// </summary>
public sealed class InMemoryAgentPromptStore : IAgentPromptStore
{
    private readonly ConcurrentDictionary<string, AgentPrompt> prompts = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task<AgentPrompt?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        prompts.TryGetValue(key, out var prompt);
        return Task.FromResult(prompt);
    }

    /// <inheritdoc />
    public Task SaveAsync(AgentPrompt prompt, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        prompts[prompt.Key] = prompt;
        return Task.CompletedTask;
    }
}

