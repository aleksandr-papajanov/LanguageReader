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
            BuildInstructions(),
            BuildInput(request),
            SchemaName: SchemaName,
            JsonSchema: BuildSchema(request),
            Model: Model,
            request.SourceText.Length,
            request.OriginalText?.Length ?? 0,
            ExpectedJsonPropertyCount: 1);
    }

    private static string BuildInput(TranslateRequest request)
    {
        return $"""
Task: translate selected text for in-place reading.

Selected text: {request.SourceText.Trim()}
Context: {NormalizeContext(request)}
Source language: {request.SourceLanguage.Trim()}
Target language: {request.TargetLanguage.Trim()}

Rules:
- Use the context to resolve ambiguity.
- Return the context-specific meaning, not dictionary form.
""";
    }

    private static string BuildInstructions()
    {
        return """
Translate for language learners.
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
                    minLength = 1
                }
            }
        };

        return JsonSerializer.Serialize(schema, JsonOptions.Options);
    }

    private static void ValidateRequest(TranslateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TargetLanguage))
        {
            throw new ValidationException("Select a learning language in Settings before translating.");
        }

        if (string.IsNullOrWhiteSpace(request.SourceText))
        {
            throw new ValidationException("Select text before translating.");
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
            SelectionKind.Word => $"Context-specific meaning of the selected word from {sourceLanguage}, translated into {targetLanguage}.",
            SelectionKind.Sentence => $"Natural sentence translation from {sourceLanguage} into {targetLanguage}.",
            SelectionKind.Paragraph => $"Natural paragraph translation from {sourceLanguage} into {targetLanguage}.",
            _ => $"Natural custom-fragment translation from {sourceLanguage} into {targetLanguage}."
        };
    }

    internal sealed record Payload(string TranslatedText);
}