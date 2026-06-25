using System.Text.Json;
using LanguageReader.Infrastructure.Ai.Models;
using LanguageReader.Infrastructure.Ai.Providers;
using LanguageReader.Infrastructure.Ai.Providers.Models;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai.Models;
using LanguageReader.Shared.Configuration;
using Microsoft.Extensions.Options;

namespace LanguageReader.Infrastructure.Ai.Execution;

public sealed class AgentHandler(
    IAiProviderClient providerClient,
    IAiModelResolver modelResolver,
    IOptions<OpenAiOptions> options) : IAiExecutionHandler
{
    private string ProviderName => string.IsNullOrWhiteSpace(options.Value.ProviderName)
        ? "Unknown"
        : options.Value.ProviderName.Trim();


    public bool CanHandle<TResult>(IAiOperation<TResult> operation)
    {
        return operation.GetType()
            .GetInterfaces()
            .Any(type => type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(IAiAgentOperation<>));
    }

    public async Task<TResult> ExecuteAsync<TResult>(
        IAiOperation<TResult> operation,
        CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAgentAsync((dynamic)operation, cancellationToken);
        return (TResult)result;
    }

    private async Task<AiOperationExecutionResult<TPayload>> ExecuteAgentAsync<TPayload>(
        IAiAgentOperation<TPayload> operation,
        CancellationToken cancellationToken)
    {
        var request = operation.BuildRequest();
        var model = modelResolver.Resolve(null);
        var messages = MapMessages(request.Messages);
        var tools = operation.GetTools();

        var firstResponse = await providerClient.SendAsync(
            new AiProviderRequest(
                messages,
                tools,
                ToolResults: [],
                ResponseFormat: AiProviderResponseFormat.Json,
                SchemaName: request.SchemaName,
                JsonSchema: request.JsonSchema,
                Model: model),
            cancellationToken);

        EnsureSuccess(request, model, firstResponse);

        var toolResults = new List<AiProviderToolResult>();
        foreach (var toolCall in firstResponse.ToolCalls)
        {
            toolResults.Add(await operation.ExecuteToolAsync(toolCall, cancellationToken));
        }

        var finalResponse = toolResults.Count == 0
            ? firstResponse
            : await providerClient.SendAsync(
                new AiProviderRequest(
                    messages,
                    Tools: [],
                    toolResults,
                    AiProviderResponseFormat.Json,
                    request.SchemaName,
                    request.JsonSchema,
                    model,
                    PreviousResponseId: firstResponse.ResponseId),
                cancellationToken);

        EnsureSuccess(request, model, finalResponse);

        var responseJson = string.IsNullOrWhiteSpace(finalResponse.StructuredJson)
            ? finalResponse.Text
            : finalResponse.StructuredJson;

        if (string.IsNullOrWhiteSpace(responseJson))
        {
            throw new InfrastructureException(
                $"{request.OperationName} agent returned an empty JSON response. Model: {model}. ResponseId: {finalResponse.ResponseId ?? "n/a"}.");
        }

        var payload = DeserializePayload<TPayload>(request, model, finalResponse, responseJson);
        var usageSource = finalResponse.Usage ?? firstResponse.Usage;
        var providerName = ProviderName;
        var pricing = AiPricingCatalog.GetPricing(providerName, model);

        var usage = AiOperationUsageFactory.Create(
            operation.OperationName,
            providerName,
            model,
            executionMode: "Agent",
            turnCount: toolResults.Count == 0 ? 1 : 2,
            toolCallCount: toolResults.Count,
            toolNames: BuildToolNames(toolResults),
            BuildPromptPreview(request.Messages, toolResults),
            responseJson,
            pricing.InputUsdPerMillionTokens,
            pricing.OutputUsdPerMillionTokens,
            usageSource);

        return new AiOperationExecutionResult<TPayload>(
            payload,
            model,
            responseJson,
            usage,
            finalResponse.ResponseId,
            finalResponse.IncompleteReason);
    }

    private static void EnsureSuccess(
        AiJsonOperationRequest request,
        string model,
        AiProviderResponse response)
    {
        if (!response.IsSuccess)
        {
            throw new InfrastructureException(
                $"{request.OperationName} agent provider request failed. Model: {model}. Error: {response.Error ?? "Unknown provider error."}");
        }
    }

    private static TPayload DeserializePayload<TPayload>(
        AiJsonOperationRequest request,
        string model,
        AiProviderResponse response,
        string responseJson)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<TPayload>(responseJson, JsonOptions.Options);
            return payload ?? throw new InfrastructureException(
                $"{request.OperationName} agent returned an empty JSON payload after deserialization. Model: {model}. ResponseId: {response.ResponseId ?? "n/a"}.");
        }
        catch (JsonException exception)
        {
            var preview = responseJson.Length > 600
                ? responseJson[..600] + "..."
                : responseJson;

            throw new InfrastructureException(
                $"{request.OperationName} agent returned invalid JSON. Model: {model}. ResponseId: {response.ResponseId ?? "n/a"}. Payload preview: {preview}",
                exception);
        }
    }

    private static IReadOnlyList<AiProviderChatMessage> MapMessages(
        IReadOnlyList<AiProviderMessage> messages)
    {
        return messages
            .Select(message => new AiProviderChatMessage(
                MapRole(message.Role),
                message.Content))
            .ToArray();
    }

    private static AiProviderMessageRole MapRole(AiMessageRole role)
    {
        return role switch
        {
            AiMessageRole.System => AiProviderMessageRole.System,
            AiMessageRole.User => AiProviderMessageRole.User,
            _ => throw new ArgumentOutOfRangeException(nameof(role))
        };
    }

    private static string BuildPromptPreview(
        IReadOnlyList<AiProviderMessage> messages,
        IReadOnlyList<AiProviderToolResult> toolResults)
    {
        var prompt = string.Join(
            Environment.NewLine + Environment.NewLine,
            messages.Select(message => $"{message.Role}: {message.Content}"));

        if (toolResults.Count == 0)
        {
            return prompt;
        }

        return prompt
            + Environment.NewLine
            + Environment.NewLine
            + string.Join(
                Environment.NewLine,
                toolResults.Select(result => $"Tool {result.ToolName ?? result.ToolCallId}: {result.OutputJson}"));
    }

    private static string? BuildToolNames(IReadOnlyList<AiProviderToolResult> toolResults)
    {
        var names = toolResults
            .Select(result => string.IsNullOrWhiteSpace(result.ToolName) ? result.ToolCallId : result.ToolName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return names.Length == 0 ? null : string.Join(", ", names);
    }
}
