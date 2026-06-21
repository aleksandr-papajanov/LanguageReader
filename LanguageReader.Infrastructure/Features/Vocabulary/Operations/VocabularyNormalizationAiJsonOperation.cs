using System.Text.Json;
using LanguageReader.Infrastructure.Agents.Json.Models;
using LanguageReader.Infrastructure.Agents.Json.Operations;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Common.Language;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Operations;

internal sealed class VocabularyNormalizationAiJsonOperation(
    VocabularyNormalizationRequest request,
    VocabularyNormalizationRules normalizationRules) : IAiJsonOperation<VocabularyNormalizationAiJsonOperation.Payload>
{
    public AiOperationKind Kind => AiOperationKind.VocabularyNormalization;

    public string ProviderName => "OpenAI";

    public AiJsonOperationRequest BuildRequest()
    {
        Validate(request);

        return new AiJsonOperationRequest(
            Kind,
            "Vocabulary normalization",
            BuildInstructions(normalizationRules),
            BuildInput(request),
            SchemaName: "vocabulary_normalization",
            JsonSchema: BuildSchema(request),
            Model: null,
            request.Text.Length + request.Translation.Length,
            request.ContextSentence?.Length ?? 0,
            ExpectedJsonPropertyCount: 1);
    }

    private static string BuildInput(VocabularyNormalizationRequest request)
    {
        var payload = new Dictionary<string, object?>
        {
            ["selectedText"] = request.Text.Trim(),
            ["knownTranslation"] = request.Translation.Trim(),
            ["sourceLanguage"] = request.SourceLanguage.Trim(),
            ["targetLanguage"] = request.TargetLanguage.Trim()
        };

        if (!string.IsNullOrWhiteSpace(request.ContextSentence))
        {
            payload["contextSentence"] = request.ContextSentence.Trim();
        }

        return JsonSerializer.Serialize(payload, JsonOptions.Options);
    }

    private static string BuildInstructions(VocabularyNormalizationRules normalizationRules)
    {
        return $$"""
Convert selectedText into the best saved vocabulary form for a language learner.

Rules:
- Return the source-language dictionary form only.
- Do not translate the text.
- Use knownTranslation and contextSentence only to preserve the intended meaning.
- Follow these source-language rules:
{{normalizationRules.DictionaryFormInstruction}}
""";
    }

    private static string BuildSchema(VocabularyNormalizationRequest request)
    {
        var sourceLanguage = request.SourceLanguage.Trim();

        var schema = new
        {
            type = "object",
            additionalProperties = false,
            required = new[] { "dictionaryForm" },
            properties = new
            {
                dictionaryForm = new
                {
                    type = "string",
                    description = $"Best source-language dictionary form for selectedText in {sourceLanguage}. Do not translate it.",
                    minLength = 1
                }
            }
        };

        return JsonSerializer.Serialize(schema, JsonOptions.Options);
    }

    private static void Validate(VocabularyNormalizationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            throw new ValidationException("Text is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Translation))
        {
            throw new ValidationException("Translation is required.");
        }

        if (string.IsNullOrWhiteSpace(request.SourceLanguage))
        {
            throw new ValidationException("Source language is required.");
        }

        if (string.IsNullOrWhiteSpace(request.TargetLanguage))
        {
            throw new ValidationException("Target language is required.");
        }
    }

    internal sealed record Payload(string DictionaryForm);
}
