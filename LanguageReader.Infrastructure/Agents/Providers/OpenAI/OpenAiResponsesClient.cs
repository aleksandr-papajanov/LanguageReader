using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using LanguageReader.Infrastructure.Agents.Core.Models;
using LanguageReader.Infrastructure.Agents.Providers.Models;
using LanguageReader.Infrastructure.Agents.Tools.Models;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Shared.Configuration;
using Microsoft.Extensions.Options;

namespace LanguageReader.Infrastructure.Agents.Providers.OpenAI;

/// <summary>
/// OpenAI Responses API implementation of the provider-neutral AI client.
/// </summary>
public sealed class OpenAiResponsesClient(
    HttpClient httpClient,
    IOptions<OpenAiOptions> options) : IAiProviderClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly OpenAiOptions openAiOptions = options.Value;

    /// <inheritdoc />
    public async Task<AiProviderResponse> SendAsync(AiProviderRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(openAiOptions.ApiKey))
        {
            throw new InfrastructureException("OpenAI API key is not configured.");
        }

        var model = string.IsNullOrWhiteSpace(request.Model)
            ? openAiOptions.DefaultModel
            : request.Model;

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new InfrastructureException("OpenAI model is not configured.");
        }

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

    private static object BuildRequestBody(AiProviderRequest request, string model)
    {
        var input = new List<object>();

        foreach (var message in request.Messages)
        {
            input.Add(new
            {
                role = message.Role,
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

        var tools = request.Tools.Select(tool => new
        {
            type = "function",
            name = tool.Name,
            description = tool.Description,
            parameters = JsonNode.Parse(tool.ParametersJsonSchema)
        }).ToArray();

        var hasTools = tools.Length > 0;

        return new
        {
            model,
            instructions = BuildInstructions(request),

            input,

            tools = hasTools ? tools : null,

            max_output_tokens = request.MaxOutputTokens,

            reasoning = IsReasoningModel(model)
                ? new { effort = hasTools ? "low" : "minimal" }
                : null,

            text = request.ResponseFormat == AgentResponseFormat.Json && !hasTools
                ? BuildTextFormat(request)
                : null
        };
    }

    private static object? BuildTextFormat(AiProviderRequest request)
    {
        if (request.ResponseFormat != AgentResponseFormat.Json)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(request.SchemaName) && !string.IsNullOrWhiteSpace(request.JsonSchema))
        {
            return new
            {
                format = new
                {
                    type = "json_schema",
                    name = request.SchemaName,
                    strict = true,
                    schema = JsonNode.Parse(request.JsonSchema)
                }
            };
        }

        return new
        {
            format = new
            {
                type = "json_object"
            }
        };
    }

    private static string? BuildInstructions(AiProviderRequest request)
    {
        if (request.ResponseFormat != AgentResponseFormat.Json)
        {
            return string.IsNullOrWhiteSpace(request.Instructions)
                ? null
                : request.Instructions;
        }

        var baseInstructions = string.IsNullOrWhiteSpace(request.Instructions)
            ? ""
            : request.Instructions.Trim();

        return baseInstructions + """

    Return valid JSON only.
    Do not include markdown.
    Do not include explanations.
    """;
    }

    private static bool IsReasoningModel(string model)
    {
        return model.StartsWith("gpt-5", StringComparison.OrdinalIgnoreCase)
            || model.StartsWith("o", StringComparison.OrdinalIgnoreCase);
    }

    private static AiProviderResponse ParseResponse(string json, AgentResponseFormat responseFormat)
    {
        var root = JsonNode.Parse(json)?.AsObject()
            ?? throw new InfrastructureException("OpenAI returned an empty response.");

        var responseId = root["id"]?.GetValue<string>();
        var incompleteReason = root["incomplete_details"]?["reason"]?.GetValue<string>();
        var toolCalls = new List<AgentToolCall>();
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
                toolCalls.Add(new AgentToolCall(
                    outputObject["call_id"]?.GetValue<string>() ?? outputObject["id"]?.GetValue<string>() ?? Guid.NewGuid().ToString("N"),
                    outputObject["name"]?.GetValue<string>() ?? string.Empty,
                    outputObject["arguments"]?.GetValue<string>() ?? "{}"));
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
        var structuredJson = responseFormat == AgentResponseFormat.Json ? textResult : null;
        var usage = ParseUsage(root["usage"]?.AsObject());

        return new AiProviderResponse(
            responseId,
            responseFormat == AgentResponseFormat.PlainText ? textResult : null,
            structuredJson,
            toolCalls,
            IsSuccess: true,
            Usage: usage,
            IncompleteReason: incompleteReason);
    }

    private static AiProviderUsage? ParseUsage(JsonObject? usageObject)
    {
        if (usageObject is null)
        {
            return null;
        }

        return new AiProviderUsage(
            ReadInt(usageObject, "input_tokens"),
            ReadInt(usageObject, "output_tokens"),
            ReadInt(usageObject, "total_tokens"),
            ReadInt(usageObject["output_tokens_details"] as JsonObject, "reasoning_tokens"),
            ReadInt(usageObject["input_tokens_details"] as JsonObject, "cached_tokens")
                ?? ReadInt(usageObject["input_tokens_details"] as JsonObject, "cache_read_input_tokens"));
    }

    private static int? ReadInt(JsonObject? source, string propertyName)
    {
        if (source is null)
        {
            return null;
        }

        if (source[propertyName] is null)
        {
            return null;
        }

        return source[propertyName]!.GetValue<int?>();
    }
}

