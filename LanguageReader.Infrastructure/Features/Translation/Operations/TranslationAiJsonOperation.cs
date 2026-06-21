using System.Text.Json;
using LanguageReader.Infrastructure.Agents.Json.Models;
using LanguageReader.Infrastructure.Agents.Json.Operations;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai.Models;

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
            Model: null,
            request.SourceText.Length,
            request.OriginalText?.Length ?? 0,
            ExpectedJsonPropertyCount: 1);
    }

    private static string BuildInput(TranslateRequest request)
    {
        var payload = new Dictionary<string, object?>
        {
            ["selectedText"] = request.SourceText.Trim(),
            ["selectionKind"] = request.SelectionKind.ToString(),
            ["sourceLanguage"] = string.IsNullOrWhiteSpace(request.SourceLanguage) ? "Unknown" : request.SourceLanguage.Trim(),
            ["targetLanguage"] = request.TargetLanguage.Trim()
        };

        if (!string.IsNullOrWhiteSpace(request.OriginalText))
        {
            payload["contextText"] = request.OriginalText.Trim();
        }

        return JsonSerializer.Serialize(payload, JsonOptions.Options);
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

    internal sealed record Payload(string TranslatedText);
}
