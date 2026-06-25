using System.Text.Json;
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
                "Gets the sentence or block context around the selected fragment.",
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
                "Translate only the selected fragment. Before answering, call get_translation_context and use its result as surrounding context."),

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

Rules:
- Translate only the selected fragment, not the full context.
- The result should fit naturally if inserted back into the context.
- Add a short note in parentheses only if the translation is unclear without it.
""";
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

    public sealed record Payload(string TranslatedText);
}
