using System.Text.Json;
using LanguageReader.Infrastructure.Ai.Execution;
using LanguageReader.Infrastructure.Ai.Models;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;

namespace LanguageReader.Infrastructure.Ai.Operations.Translation;

public sealed class SimpleContextualTranslationOperation(
    TranslateRequest request) : IAiJsonOperation<SimpleContextualTranslationOperation.Payload>
{
    private const string SchemaName = "translation_result";

    public string OperationName => "Simple contextual translation";

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

    private static IReadOnlyList<AiProviderMessage> BuildMessages(TranslateRequest request)
    {
        return
        [
            new(
                AiMessageRole.System,
                "Translate only the selected fragment. Use the provided surrounding context directly; do not translate the full context."),

            new(
                AiMessageRole.User,
                BuildInput(request))
        ];
    }

    private static string BuildInput(TranslateRequest request)
    {
        var context = string.IsNullOrWhiteSpace(request.OriginalText)
            ? "none"
            : request.OriginalText.Trim();

        return $"""
Selected fragment: {request.SourceText.Trim()}
Source language: {request.SourceLanguage.Trim()}
Target language: {request.TargetLanguage.Trim()}
Selection kind: {request.SelectionKind}
Surrounding context: {context}

Rules:
- Translate only the selected fragment, not the full context.
- Use surrounding context only to resolve meaning, grammar, tone, idioms, gender, number, tense, and word sense.
- The result should fit naturally if inserted back into the surrounding context.
- Add a short note in parentheses only if the translation is unclear without it.
""";
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
