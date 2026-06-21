using System.Text.Json;
using LanguageReader.Infrastructure.Agents.Json.Models;
using LanguageReader.Infrastructure.Agents.Json.Operations;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Operations;

internal sealed class VocabularyExampleAiJsonOperation(
    VocabularyExampleGenerationRequest request) : IAiJsonOperation<VocabularyExampleAiJsonOperation.Payload>
{
    public AiOperationKind Kind => AiOperationKind.VocabularyExampleGeneration;

    public string ProviderName => "OpenAI";

    public AiJsonOperationRequest BuildRequest()
    {
        Validate(request);

        var input = BuildInput(request);
        return new AiJsonOperationRequest(
            Kind,
            "Vocabulary example generation",
            BuildInstructions(),
            input,
            SchemaName: "vocabulary_example",
            JsonSchema: BuildSchema(request),
            Model: null,
            request.Word.Length + request.Translation.Length,
            request.ContextSentence?.Length ?? 0,
            ExpectedJsonPropertyCount: 2);
    }

    private static string BuildInput(VocabularyExampleGenerationRequest request)
    {
        var payload = new Dictionary<string, object?>
        {
            ["word"] = request.Word.Trim(),
            ["knownTranslation"] = request.Translation.Trim(),
            ["wordLanguage"] = request.WordLanguage.Trim(),
            ["translationLanguage"] = request.TranslationLanguage.Trim()
        };

        if (!string.IsNullOrWhiteSpace(request.ContextSentence))
        {
            payload["contextSentence"] = request.ContextSentence.Trim();
        }

        return JsonSerializer.Serialize(payload, JsonOptions.Options);
    }

    private static string BuildInstructions()
    {
        return """
Generate one learner-friendly example sentence for a saved word.

Rules:
- Write one natural example sentence using the selected word correctly.
- If contextSentence exists, stay close to its meaning and register without copying it verbatim.
- Provide one short learner-friendly translation or paraphrase.
""";
    }

    private static string BuildSchema(VocabularyExampleGenerationRequest request)
    {
        var wordLanguage = request.WordLanguage.Trim();
        var translationLanguage = request.TranslationLanguage.Trim();

        var schema = new
        {
            type = "object",
            additionalProperties = false,
            required = new[] { "text", "translation" },
            properties = new
            {
                text = new
                {
                    type = "string",
                    description = $"One natural learner-friendly example sentence in {wordLanguage} that uses the selected word correctly.",
                    minLength = 1
                },
                translation = new
                {
                    type = "string",
                    description = $"Translation of the generated example sentence in {translationLanguage}.",
                    minLength = 1
                }
            }
        };

        return JsonSerializer.Serialize(schema, JsonOptions.Options);
    }

    private static void Validate(VocabularyExampleGenerationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Word))
        {
            throw new ValidationException("Word is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Translation))
        {
            throw new ValidationException("Translation is required.");
        }

        if (string.IsNullOrWhiteSpace(request.WordLanguage))
        {
            throw new ValidationException("Word language is required.");
        }

        if (string.IsNullOrWhiteSpace(request.TranslationLanguage))
        {
            throw new ValidationException("Translation language is required.");
        }
    }

    internal sealed record Payload(
        string Text,
        string Translation);
}
