using LanguageReader.Infrastructure.Agents.Core.Models;

namespace LanguageReader.Infrastructure.Agents.Core;

/// <summary>
/// Creates configured agent instances.
/// </summary>
public interface IAgentFactory
{
    /// <summary>
    /// Creates an agent for the supplied definition.
    /// </summary>
    IAgent CreateAgent(AgentDefinition definition);
}

