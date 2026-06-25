using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using LanguageReader.Infrastructure.Ai.Providers;
using LanguageReader.Infrastructure.Ai.Providers.Models;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Shared.Configuration;
using Microsoft.Extensions.Options;

namespace LanguageReader.Infrastructure.Ai.Providers.OpenAI;

public sealed class OpenAiResponsesClient(
    HttpClient httpClient,
    IOptions<OpenAiOptions> options) : IAiProviderClient
{
    private const string GlobalSystemMessage = """
You are a language-learning assistant.

Follow the requested task exactly.
Return only valid JSON matching the provided schema.
""";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly OpenAiOptions openAiOptions = options.Value;

    public async Task<AiProviderResponse> SendAsync(
        AiProviderRequest request,
        CancellationToken cancellationToken = default)
    {
        var model = ResolveModel(request);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "responses");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", openAiOptions.ApiKey);
        httpRequest.Content = JsonContent.Create(BuildRequestBody(request, model), options: JsonOptions);

        using var response = await httpClient.SendAsync(httpRequest, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return new AiProviderResponse(
                null,
                null,
                null,
                [],
                IsSuccess: false,
                Error: json);
        }

        return ParseResponse(json, request.ResponseFormat);
    }

    private string ResolveModel(AiProviderRequest request)
    {
        if (string.IsNullOrWhiteSpace(openAiOptions.ApiKey))
        {
            throw new InfrastructureException("OpenAI API key is not configured.");
        }

        var model = string.IsNullOrWhiteSpace(request.Model)
            ? openAiOptions.DefaultModel
            : request.Model.Trim();

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new InfrastructureException("OpenAI model is not configured.");
        }

        return model;
    }

    private static object BuildRequestBody(AiProviderRequest request, string model)
    {
        var hasTools = request.Tools.Count > 0;

        return new
        {
            model,
            input = BuildInput(request),
            previous_response_id = request.PreviousResponseId,
            tools = hasTools ? BuildTools(request) : null,
            max_output_tokens = request.MaxOutputTokens,
            reasoning = IsReasoningModel(model)
                ? new { effort = hasTools ? "low" : "minimal" }
                : null,
            text = request.ResponseFormat == AiProviderResponseFormat.Json && !hasTools
                ? BuildTextFormat(request)
                : null
        };
    }

    private static List<object> BuildInput(AiProviderRequest request)
    {
        var input = new List<object>
        {
            new
            {
                role = "system",
                content = GlobalSystemMessage
            }
        };

        foreach (var message in request.Messages)
        {
            input.Add(new
            {
                role = ToRole(message.Role),
                content = message.Content
            });
        }

        foreach (var result in request.ToolResults)
        {
            input.Add(new
            {
                type = "function_call_output",
                call_id = result.ToolCallId,
                output = result.OutputJson
            });
        }

        return input;
    }

    private static string ToRole(AiProviderMessageRole  role)
    {
        return role switch
        {
            AiProviderMessageRole .System => "system",
            AiProviderMessageRole .User => "user",
            AiProviderMessageRole .Assistant => "assistant",
            _ => throw new ArgumentOutOfRangeException(nameof(role))
        };
    }

    private static object[] BuildTools(AiProviderRequest request)
    {
        return request.Tools
            .Select(tool => new
            {
                type = "function",
                name = tool.Name,
                description = tool.Description,
                parameters = ParseJson(tool.ParametersJsonSchema, $"Invalid schema for tool '{tool.Name}'.")
            })
            .ToArray();
    }

    private static object? BuildTextFormat(AiProviderRequest request)
    {
        if (request.ResponseFormat != AiProviderResponseFormat.Json)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(request.SchemaName) ||
            string.IsNullOrWhiteSpace(request.JsonSchema))
        {
            return new
            {
                format = new
                {
                    type = "json_object"
                }
            };
        }

        return new
        {
            format = new
            {
                type = "json_schema",
                name = request.SchemaName,
                strict = true,
                schema = ParseJson(request.JsonSchema, $"Invalid JSON schema '{request.SchemaName}'.")
            }
        };
    }

    private static JsonNode ParseJson(string json, string errorMessage)
    {
        try
        {
            return JsonNode.Parse(json)
                ?? throw new InfrastructureException(errorMessage);
        }
        catch (JsonException exception)
        {
            throw new InfrastructureException(errorMessage, exception);
        }
    }

    private static bool IsReasoningModel(string model)
    {
        return model.StartsWith("gpt-5", StringComparison.OrdinalIgnoreCase)
            || model.StartsWith("o", StringComparison.OrdinalIgnoreCase);
    }

    private static AiProviderResponse ParseResponse(
        string json,
        AiProviderResponseFormat responseFormat)
    {
        var root = JsonNode.Parse(json)?.AsObject()
            ?? throw new InfrastructureException("OpenAI returned an empty response.");

        var responseId = root["id"]?.GetValue<string>();
        var incompleteReason = root["incomplete_details"]?["reason"]?.GetValue<string>();

        var toolCalls = new List<AiProviderToolCall>();
        var textParts = new List<string>();

        foreach (var output in root["output"]?.AsArray() ?? [])
        {
            if (output is null)
            {
                continue;
            }

            var outputObject = output.AsObject();
            var type = outputObject["type"]?.GetValue<string>();

            if (type == "function_call")
            {
                toolCalls.Add(ParseToolCall(outputObject));
                continue;
            }

            foreach (var content in outputObject["content"]?.AsArray() ?? [])
            {
                var text = content?["text"]?.GetValue<string>();

                if (!string.IsNullOrWhiteSpace(text))
                {
                    textParts.Add(text);
                }
            }
        }

        var textResult = string.Join(Environment.NewLine, textParts).Trim();
        var usage = ParseUsage(root["usage"]?.AsObject());

        return new AiProviderResponse(
            responseId,
            responseFormat == AiProviderResponseFormat.PlainText ? textResult : null,
            responseFormat == AiProviderResponseFormat.Json ? textResult : null,
            toolCalls,
            IsSuccess: true,
            Usage: usage,
            IncompleteReason: incompleteReason);
    }

    private static AiProviderToolCall ParseToolCall(JsonObject outputObject)
    {
        return new AiProviderToolCall(
            outputObject["call_id"]?.GetValue<string>()
                ?? outputObject["id"]?.GetValue<string>()
                ?? Guid.NewGuid().ToString("N"),
            outputObject["name"]?.GetValue<string>() ?? string.Empty,
            outputObject["arguments"]?.GetValue<string>() ?? "{}");
    }

    private static AiProviderUsage? ParseUsage(JsonObject? usageObject)
    {
        if (usageObject is null)
        {
            return null;
        }

        var inputDetails = usageObject["input_tokens_details"] as JsonObject;
        var outputDetails = usageObject["output_tokens_details"] as JsonObject;

        return new AiProviderUsage(
            ReadInt(usageObject, "input_tokens"),
            ReadInt(usageObject, "output_tokens"),
            ReadInt(usageObject, "total_tokens"),
            ReadInt(outputDetails, "reasoning_tokens"),
            ReadInt(inputDetails, "cached_tokens")
                ?? ReadInt(inputDetails, "cache_read_input_tokens"));
    }

    private static int? ReadInt(JsonObject? source, string propertyName)
    {
        return source?[propertyName]?.GetValue<int?>();
    }
}
