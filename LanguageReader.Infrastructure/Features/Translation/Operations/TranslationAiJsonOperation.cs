using System.Text.Json;
using LanguageReader.Infrastructure.Agents.Json.Models;
using LanguageReader.Infrastructure.Agents.Json.Operations;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;

namespace LanguageReader.Infrastructure.Features.Translation.Operations;

internal sealed class TranslationAiJsonOperation(
    TranslateRequest request) : IAiJsonOperation<TranslationAiJsonOperation.Payload>
{
    public AiOperationKind Kind => AiOperationKind.Translation;

    public string ProviderName => "OpenAI";

    public AiJsonOperationRequest BuildRequest()
    {
        ValidateRequest(request);

        var input = BuildInput(request);
        return new AiJsonOperationRequest(
            Kind,
            "Translation",
            BuildInstructions(),
            input,
            SchemaName: "translation_result",
            JsonSchema: BuildSchema(request),
            Model: "gpt-5-mini",
            request.SourceText.Length,
            request.OriginalText?.Length ?? 0,
            ExpectedJsonPropertyCount: 1);
    }

    private static string BuildInput(TranslateRequest request)
    {
        return $$"""
Selected text:
{{request.SourceText.Trim()}}

Context:
{{NormalizeContext(request)}}

Source language:
{{(string.IsNullOrWhiteSpace(request.SourceLanguage) ? "Unknown" : request.SourceLanguage.Trim())}}

Target language:
{{request.TargetLanguage.Trim()}}
""";
    }

    private static string BuildInstructions()
    {
        return """
Translate selectedText for a language learner.
Always translate it as it should naturally fit in contextText.
Preserve the meaning, tone, and grammar implied by contextText.
Return only the target-language wording, not explanations or dictionary forms.
sourceLanguage is the language of selectedText.
targetLanguage is the language you must translate into.
Never return sourceLanguage text unless sourceLanguage and targetLanguage are the same language.
""";
    }

    private static string BuildSchema(TranslateRequest request)
    {
        var sourceLanguage = string.IsNullOrWhiteSpace(request.SourceLanguage)
            ? "the source language"
            : request.SourceLanguage.Trim();
        var targetLanguage = request.TargetLanguage.Trim();

        var properties = new Dictionary<string, object>
        {
            ["translatedText"] = new
            {
                type = "string",
                description = $"Natural translation of the selected text from {sourceLanguage} into {targetLanguage}.",
                minLength = 1
            }
        };

        var schema = new
        {
            type = "object",
            additionalProperties = false,
            required = new[] { "translatedText" },
            properties
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

    internal sealed record Payload(string TranslatedText);
}
