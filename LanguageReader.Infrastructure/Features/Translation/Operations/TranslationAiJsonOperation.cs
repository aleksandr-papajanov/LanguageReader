using System.Text.Json;
using LanguageReader.Infrastructure.Agents.Json.Models;
using LanguageReader.Infrastructure.Agents.Json.Operations;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;

namespace LanguageReader.Infrastructure.Features.Translation.Operations;

internal sealed class TranslationAiJsonOperation(
    TranslateRequest request) : IAiJsonOperation<TranslationAiJsonOperation.Payload>
{
    private const string OperationName = "Translation";
    private const string SchemaName = "translation_result";
    private const string Model = "gpt-5-mini";

    public AiOperationKind Kind => AiOperationKind.Translation;

    public string ProviderName => "OpenAI";

    public AiJsonOperationRequest BuildRequest()
    {
        ValidateRequest(request);

        return new AiJsonOperationRequest(
            Kind,
            OperationName,
            BuildMessages(request),
            SchemaName: SchemaName,
            JsonSchema: BuildSchema(request),
            Model: Model,
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
                "Translate only the selected fragment as it fits inside the context."),

            new(
                AiMessageRole.User,
                BuildInput(request))
        ];
    }

    private static string BuildInput(TranslateRequest request)
    {
        return $"""
Selected fragment: {request.SourceText.Trim()}
Context: {NormalizeContext(request)}
Source language: {request.SourceLanguage.Trim()}
Target language: {request.TargetLanguage.Trim()}

Rules:
- Translate only the selected fragment, not the full context.
- The result should fit naturally if inserted back into the context.
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

    private static string NormalizeContext(TranslateRequest request)
    {
        return string.IsNullOrWhiteSpace(request.OriginalText)
            ? request.SourceText.Trim()
            : request.OriginalText.Trim();
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

    internal sealed record Payload(string TranslatedText);
}