using System.Text.Json;
using LanguageReader.Infrastructure.Agents.Core.Models;
using LanguageReader.Infrastructure.Agents.Json.Models;
using LanguageReader.Infrastructure.Agents.Providers;
using LanguageReader.Infrastructure.Agents.Providers.Models;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;

namespace LanguageReader.Infrastructure.Agents.Json;

/// <summary>
/// Sends direct JSON AI requests and validates the returned payload.
/// </summary>
public sealed class AiJsonRequestService(
    IAiProviderClient providerClient,
    IAiModelResolver modelResolver) : IAiJsonRequestService
{
    public async Task<AiJsonOperationResult<TPayload>> CompleteAsync<TPayload>(
        AiJsonOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = modelResolver.Resolve(request.Model);

        var providerResponse = await providerClient.SendAsync(new AiProviderRequest(
            Instructions: request.Instructions,
            Messages: [new AgentMessage("user", request.InputJson)],
            Tools: [],
            ToolResults: [],
            ResponseFormat: AgentResponseFormat.Json,
            SchemaName: request.SchemaName,
            JsonSchema: request.JsonSchema,
            Model: model), cancellationToken);

        if (!providerResponse.IsSuccess)
        {
            throw new InfrastructureException(
                $"{request.OperationName} provider request failed. Model: {model}. Error: {providerResponse.Error ?? "Unknown provider error."}");
        }

        var responseJson = string.IsNullOrWhiteSpace(providerResponse.StructuredJson)
            ? providerResponse.Text
            : providerResponse.StructuredJson;

        if (string.IsNullOrWhiteSpace(responseJson))
        {
            throw new InfrastructureException(
                $"{request.OperationName} returned an empty JSON response. Model: {model}. ResponseId: {providerResponse.ResponseId ?? "n/a"}. IncompleteReason: {providerResponse.IncompleteReason ?? "n/a"}.");
        }

        try
        {
            var payload = JsonSerializer.Deserialize<TPayload>(responseJson, JsonOptions.Options);
            if (payload is null)
            {
                throw new InfrastructureException(
                    $"{request.OperationName} returned an empty JSON payload after deserialization. Model: {model}. ResponseId: {providerResponse.ResponseId ?? "n/a"}.");
            }

            return new AiJsonOperationResult<TPayload>(
                payload,
                responseJson,
                model,
                providerResponse.ResponseId,
                providerResponse.Usage,
                providerResponse.IncompleteReason);
        }
        catch (JsonException exception)
        {
            var preview = responseJson.Length > 600
                ? responseJson[..600] + "..."
                : responseJson;

            throw new InfrastructureException(
                $"{request.OperationName} returned invalid or truncated JSON. Model: {model}. ResponseId: {providerResponse.ResponseId ?? "n/a"}. IncompleteReason: {providerResponse.IncompleteReason ?? "n/a"}. Payload preview: {preview}",
                exception);
        }
    }
}
