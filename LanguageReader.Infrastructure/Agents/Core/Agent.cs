using LanguageReader.Infrastructure.Agents.Core.Models;
using LanguageReader.Infrastructure.Agents.Providers;
using LanguageReader.Infrastructure.Agents.Providers.Models;
using LanguageReader.Infrastructure.Agents.Tools;
using LanguageReader.Infrastructure.Agents.Tools.Models;

namespace LanguageReader.Infrastructure.Agents.Core;

/// <summary>
/// Default provider-backed agent implementation.
/// </summary>
public sealed class Agent(
    AgentDefinition definition,
    IAiProviderClient providerClient,
    IToolDispatcher toolDispatcher) : IAgent
{
    /// <inheritdoc />
    public async Task<AgentRunResult> RunAsync(AgentRunRequest request, CancellationToken cancellationToken = default)
    {
        var messages = new List<AgentMessage>(request.Messages ?? []);
        if (!string.IsNullOrWhiteSpace(request.Input))
        {
            messages.Add(new AgentMessage("user", request.Input));
        }

        var allToolCalls = new List<AgentToolCall>();
        var allToolResults = new List<AgentToolResult>();
        var pendingToolResults = new List<AgentToolResult>();
        string? providerResponseId = null;

        for (var iteration = 0; iteration <= definition.MaxToolIterations; iteration++)
        {
            var providerRequest = new AiProviderRequest(
                definition.Instructions,
                messages,
                definition.Tools,
                pendingToolResults,
                definition.ResponseFormat,
                SchemaName: null,
                JsonSchema: null,
                definition.Model);

            var response = await providerClient.SendAsync(providerRequest, cancellationToken);
            providerResponseId = response.ResponseId ?? providerResponseId;

            if (!response.IsSuccess)
            {
                return new AgentRunResult(
                    AgentRunStatus.Failed,
                    response.Text,
                    response.StructuredJson,
                    allToolCalls,
                    allToolResults,
                    providerResponseId,
                    response.Error);
            }

            if (response.ToolCalls.Count == 0)
            {
                return new AgentRunResult(
                    AgentRunStatus.Completed,
                    response.Text,
                    response.StructuredJson,
                    allToolCalls,
                    allToolResults,
                    providerResponseId);
            }

            if (iteration == definition.MaxToolIterations)
            {
                return new AgentRunResult(
                    AgentRunStatus.MaxIterationsReached,
                    response.Text,
                    response.StructuredJson,
                    allToolCalls,
                    allToolResults,
                    providerResponseId);
            }

            pendingToolResults.Clear();
            foreach (var call in response.ToolCalls)
            {
                allToolCalls.Add(call);
                var toolResult = await toolDispatcher.DispatchAsync(call, cancellationToken);
                pendingToolResults.Add(toolResult);
                allToolResults.Add(toolResult);
            }

            if (definition.CompleteOnToolResult && pendingToolResults.Any(result => !result.IsError))
            {
                return new AgentRunResult(
                    AgentRunStatus.Completed,
                    response.Text,
                    response.StructuredJson,
                    allToolCalls,
                    allToolResults,
                    providerResponseId);
            }
        }

        return new AgentRunResult(
            AgentRunStatus.MaxIterationsReached,
            null,
            null,
            allToolCalls,
            allToolResults,
            providerResponseId);
    }
}

