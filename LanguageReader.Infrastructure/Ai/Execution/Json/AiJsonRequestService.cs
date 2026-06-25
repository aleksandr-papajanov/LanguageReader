using System.Text.Json;
using LanguageReader.Infrastructure.Ai.Models;
using LanguageReader.Infrastructure.Ai.Providers;
using LanguageReader.Infrastructure.Ai.Providers.Models;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;

namespace LanguageReader.Infrastructure.Ai.Execution;

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
        var model = modelResolver.Resolve(null);

        var providerResponse = await providerClient.SendAsync(
            new AiProviderRequest(
                Messages: MapMessages(request.Messages),
                Tools: [],
                ToolResults: [],
                ResponseFormat: AiProviderResponseFormat.Json,
                SchemaName: request.SchemaName,
                JsonSchema: request.JsonSchema,
                Model: model),
            cancellationToken);

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
}
