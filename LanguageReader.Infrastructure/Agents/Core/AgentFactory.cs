using LanguageReader.Infrastructure.Agents.Core.Models;
using LanguageReader.Infrastructure.Agents.Providers;
using LanguageReader.Infrastructure.Agents.Tools;

namespace LanguageReader.Infrastructure.Agents.Core;

/// <summary>
/// Creates provider-backed agents.
/// </summary>
public sealed class AgentFactory(
    IAiProviderClient providerClient,
    IToolDispatcher toolDispatcher) : IAgentFactory
{
    /// <inheritdoc />
    public IAgent CreateAgent(AgentDefinition definition)
    {
        return new Agent(definition, providerClient, toolDispatcher);
    }
}

