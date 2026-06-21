using System.Text.Json;
using LanguageReader.Infrastructure.Agents.Json.Models;
using LanguageReader.Infrastructure.Agents.Json.Operations;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai.Models;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Operations;

internal sealed class VocabularyAutofillAiJsonOperation(
    VocabularyAutofillRequest request) : IAiJsonOperation<VocabularyAutofillAiJsonOperation.Payload>
{
    public AiOperationKind Kind => AiOperationKind.VocabularyAutofill;

    public string ProviderName => "OpenAI";

    public AiJsonOperationRequest BuildRequest()
    {
        Validate(request);

        var input = BuildInput(request);
        return new AiJsonOperationRequest(
            Kind,
            "Vocabulary autofill",
            BuildInstructions(),
            input,
            SchemaName: "vocabulary_autofill",
            JsonSchema: BuildSchema(request),
            Model: null,
            request.Word.Length + request.Translation.Length,
            request.ContextSentence?.Length ?? 0,
            ExpectedJsonPropertyCount: 9);
    }

    private static string BuildInput(VocabularyAutofillRequest request)
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
Enrich one saved vocabulary dictionary form for a language learner.

Rules:
- Treat word as already normalized to the best dictionary form.
- Base every field on word, not on an inflected seen form.
- Use contextSentence only for learner-friendly nuance and examples.
""";
    }

    private static string BuildSchema(VocabularyAutofillRequest request)
    {
        var wordLanguage = request.WordLanguage.Trim();
        var translationLanguage = request.TranslationLanguage.Trim();

        var schema = new
        {
            type = "object",
            additionalProperties = false,
            required = new[]
            {
                "primaryTranslation",
                "description",
                "alternativeTranslations",
                "partOfSpeech",
                "frequencyScore",
                "notes",
                "synonyms",
                "antonyms",
                "related"
            },
            properties = new
            {
                primaryTranslation = new
                {
                    type = "string",
                    description = $"The best short canonical translation of word in {translationLanguage}. Use the known translation only as guidance.",
                    minLength = 1
                },
                description = new
                {
                    type = "string",
                    description = $"One short learner-friendly definition written entirely in {wordLanguage}. Do not translate the definition.",
                    minLength = 1
                },
                alternativeTranslations = new
                {
                    type = "array",
                    description = $"Other possible short translations in {translationLanguage}. Do not repeat primaryTranslation.",
                    minItems = 0,
                    maxItems = 3,
                    items = new
                    {
                        type = "string",
                        minLength = 1
                    }
                },
                partOfSpeech = new
                {
                    type = "string",
                    description = "A short part-of-speech label in English, for example noun, verb, adjective, adverb, phrase, preposition, conjunction, pronoun, or interjection.",
                    minLength = 1,
                    maxLength = 24
                },
                frequencyScore = new
                {
                    type = "integer",
                    description = "Estimated everyday frequency from 0 to 100. 0 means extremely rare, 100 means extremely common.",
                    minimum = 0,
                    maximum = 100
                },
                notes = new
                {
                    type = "string",
                    description = $"One practical learner note in {translationLanguage}. Use an empty string if there is nothing useful to add."
                },
                synonyms = new
                {
                    type = "array",
                    description = $"Synonyms of word in {wordLanguage}. Do not include translations.",
                    minItems = 0,
                    maxItems = 4,
                    items = new
                    {
                        type = "string",
                        minLength = 1
                    }
                },
                antonyms = new
                {
                    type = "array",
                    description = $"Antonyms of word in {wordLanguage}. Do not include translations.",
                    minItems = 0,
                    maxItems = 4,
                    items = new
                    {
                        type = "string",
                        minLength = 1
                    }
                },
                related = new
                {
                    type = "array",
                    description = $"Closely related words or expressions in {wordLanguage}. Do not include translations.",
                    minItems = 0,
                    maxItems = 4,
                    items = new
                    {
                        type = "string",
                        minLength = 1
                    }
                }
            }
        };

        return JsonSerializer.Serialize(schema, JsonOptions.Options);
    }

    private static void Validate(VocabularyAutofillRequest request)
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
        string PrimaryTranslation,
        string Description,
        IReadOnlyList<string> AlternativeTranslations,
        string PartOfSpeech,
        int FrequencyScore,
        string Notes,
        IReadOnlyList<string> Synonyms,
        IReadOnlyList<string> Antonyms,
        IReadOnlyList<string> Related);
}
