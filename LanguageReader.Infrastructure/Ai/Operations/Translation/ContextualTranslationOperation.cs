using System.Text.Json;
using System.Text.Json.Serialization;
using LanguageReader.Infrastructure.Ai.Execution;
using LanguageReader.Infrastructure.Ai.Models;
using LanguageReader.Infrastructure.Ai.Operations.Translation.Tools;
using LanguageReader.Infrastructure.Ai.Providers.Models;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;

namespace LanguageReader.Infrastructure.Ai.Operations.Translation;

public sealed class ContextualTranslationOperation(
    TranslateRequest request,
    TranslationContextTool contextTool) : IAiAgentOperation<ContextualTranslationOperation.Payload>
{
    private const string SchemaName = "translation_result";
    private const string ContextToolName = "get_translation_context";

    public string OperationName => "Contextual translation";

    public AiJsonOperationRequest BuildRequest()
    {
        ValidateRequest(request);

        return new AiJsonOperationRequest(
            OperationName,
            BuildMessages(request),
            SchemaName: SchemaName,
            JsonSchema: BuildSchema(request),
            request.SourceText.Length,
            request.OriginalText?.Length ?? 0,
            ExpectedJsonPropertyCount: 1);
    }

    public IReadOnlyList<AiProviderToolDefinition> GetTools()
    {
        return
        [
            new AiProviderToolDefinition(
                ContextToolName,
                "Gets extended surrounding context: up to two sentences before and after the selected fragment.",
                BuildContextToolSchema())
        ];
    }

    public Task<AiProviderToolResult> ExecuteToolAsync(
        AiProviderToolCall toolCall,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!string.Equals(toolCall.Name, ContextToolName, StringComparison.Ordinal))
        {
            throw new InfrastructureException($"Unsupported translation tool '{toolCall.Name}'.");
        }

        var context = contextTool.GetContext(request);
        var json = JsonSerializer.Serialize(context, JsonOptions.Options);

        return Task.FromResult(new AiProviderToolResult(
            toolCall.Id,
            json,
            ToolName: ContextToolName));
    }

    private static IReadOnlyList<AiProviderMessage> BuildMessages(TranslateRequest request)
    {
        return
        [
            new(
                AiMessageRole.System,
                "Translate only the selected fragment. Use the provided basic context first. Call get_translation_context only when extra surrounding sentences are needed to resolve meaning."),

            new(
                AiMessageRole.User,
                BuildInput(request))
        ];
    }

    private static string BuildInput(TranslateRequest request)
    {
        return $"""
Selected fragment: {request.SourceText.Trim()}
Source language: {request.SourceLanguage.Trim()}
Target language: {request.TargetLanguage.Trim()}
Basic context: {BuildBasicContext(request)}

Rules:
- Translate only the selected fragment, not the full context.
- Use the basic context immediately; do not ignore it.
- If the meaning is ambiguous, call get_translation_context for extra surrounding sentences.
- The result should fit naturally if inserted back into the context.
- Add a short note in parentheses only if the translation is unclear without it.
""";
    }

    private static string BuildBasicContext(TranslateRequest request)
    {
        return string.IsNullOrWhiteSpace(request.OriginalText)
            ? request.SourceText.Trim()
            : request.OriginalText.Trim();
    }

    private static string BuildContextToolSchema()
    {
        var schema = new
        {
            type = "object",
            additionalProperties = false,
            properties = new { }
        };

        return JsonSerializer.Serialize(schema, JsonOptions.Options);
    }

    private static string BuildSchema(TranslateRequest request)
    {
        var schema = new
        {
            type = "object",
            additionalProperties = false,
            required = new[] { "translatedText" },
            properties = new
            {
                translatedText = new
                {
                    type = "string",
                    description = BuildTranslationDescription(
                        request.SelectionKind,
                        request.SourceLanguage.Trim(),
                        request.TargetLanguage.Trim()),
                    minLength = 1,
                    maxLength = 300
                }
            }
        };

        return JsonSerializer.Serialize(schema, JsonOptions.Options);
    }

    private static void ValidateRequest(TranslateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SourceText))
        {
            throw new ValidationException("Select text before translating.");
        }

        if (string.IsNullOrWhiteSpace(request.SourceLanguage))
        {
            throw new ValidationException("Source language is required.");
        }

        if (string.IsNullOrWhiteSpace(request.TargetLanguage))
        {
            throw new ValidationException("Select a learning language in Settings before translating.");
        }
    }

    private static string BuildTranslationDescription(
        SelectionKind selectionKind,
        string sourceLanguage,
        string targetLanguage)
    {
        return selectionKind switch
        {
            SelectionKind.Word =>
                $"Translation of only the selected word from {sourceLanguage} into {targetLanguage}, adjusted to fit the context.",

            SelectionKind.Sentence =>
                $"Translation of only the selected sentence from {sourceLanguage} into {targetLanguage}.",

            SelectionKind.Paragraph =>
                $"Translation of only the selected paragraph from {sourceLanguage} into {targetLanguage}.",

            _ =>
                $"Translation of only the selected fragment from {sourceLanguage} into {targetLanguage}, adjusted to fit the context."
        };
    }

    public sealed record Payload(
        [property: JsonPropertyName("translatedText")]
        string TranslatedText
    );
}
