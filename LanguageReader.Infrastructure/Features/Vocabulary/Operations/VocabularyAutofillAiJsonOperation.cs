using System.Text;
using System.Text.Json;
using LanguageReader.Infrastructure.Agents.Json.Models;
using LanguageReader.Infrastructure.Agents.Json.Operations;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Operations;

internal sealed class VocabularyAutofillAiJsonOperation(
    VocabularyAutofillRequest request) : IAiJsonOperation<VocabularyAutofillAiJsonOperation.Payload>
{
    private const string OperationName = "Vocabulary autofill";
    private const string SchemaName = "vocabulary_autofill";
    private const string Model = "gpt-5-mini";

    public AiOperationKind Kind => AiOperationKind.VocabularyAutofill;

    public string ProviderName => "OpenAI";

    public AiJsonOperationRequest BuildRequest()
    {
        Validate(request);

        return new AiJsonOperationRequest(
            Kind,
            OperationName,
            BuildInstructions(),
            BuildInput(request),
            SchemaName: SchemaName,
            JsonSchema: BuildSchema(request),
            Model: Model,
            request.Word.Length + request.Translation.Length,
            request.ContextSentence?.Length ?? 0,
            ExpectedJsonPropertyCount: 9);
    }

    private static string BuildInput(VocabularyAutofillRequest request)
    {
        var context = string.IsNullOrWhiteSpace(request.ContextSentence)
            ? "none"
            : request.ContextSentence.Trim();

        return $"""
Task: enrich vocabulary entry.

Word: {request.Word.Trim()}
Word language: {request.WordLanguage.Trim()}
Known translation: {request.Translation.Trim()}
Translation language: {request.TranslationLanguage.Trim()}
Context: {context}

Rules:
- Word is already dictionary form.
- Use context and known translation only for nuance.
""";
    }

    private static string BuildInstructions()
    {
        return """
You create concise vocabulary data for language learners.
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
                "partOfSpeech",
                "primaryTranslation",
                "alternativeTranslations",
                "description",
                "synonyms",
                "antonyms",
                "related",
                "frequencyScore",
                "notes"
            },
            properties = new
            {
                partOfSpeech = new
                {
                    type = "string",
                    description = "Short English part-of-speech label: noun, verb, adjective, adverb, phrase, preposition, conjunction, pronoun, interjection, etc.",
                    minLength = 1,
                    maxLength = 24
                },
                primaryTranslation = new
                {
                    type = "string",
                    description = $"Best short canonical translation in {translationLanguage}. Use the known translation as guidance, but correct it if needed.",
                    minLength = 1,
                    maxLength = 80
                },
                alternativeTranslations = new
                {
                    type = "array",
                    description = $"Other short translations in {translationLanguage}. Do not repeat primaryTranslation.",
                    minItems = 0,
                    maxItems = 3,
                    items = new
                    {
                        type = "string",
                        minLength = 1,
                        maxLength = 80
                    }
                },
                description = new
                {
                    type = "string",
                    description = $"Short learner-friendly definition in {translationLanguage}. Do not use {wordLanguage} except for the word itself if needed.",
                    minLength = 1,
                    maxLength = 220
                },
                synonyms = new
                {
                    type = "array",
                    description = $"Synonyms in {wordLanguage}. Do not include translations. Leave empty only when no good items exist.",
                    minItems = 0,
                    maxItems = 4,
                    items = new
                    {
                        type = "string",
                        minLength = 1,
                        maxLength = 64
                    }
                },
                antonyms = new
                {
                    type = "array",
                    description = $"Antonyms in {wordLanguage}. Do not include translations. Leave empty only when no good items exist.",
                    minItems = 0,
                    maxItems = 4,
                    items = new
                    {
                        type = "string",
                        minLength = 1,
                        maxLength = 64
                    }
                },
                related = new
                {
                    type = "array",
                    description = $"Closely related words or expressions in {wordLanguage}. Do not include translations. Leave empty only when no good items exist.",
                    minItems = 0,
                    maxItems = 4,
                    items = new
                    {
                        type = "string",
                        minLength = 1,
                        maxLength = 64
                    }
                },
                frequencyScore = new
                {
                    type = "integer",
                    description = "Estimated everyday frequency from 0 to 100. 0 = extremely rare, 100 = extremely common.",
                    minimum = 0,
                    maximum = 100
                },
                notes = new
                {
                    type = "string",
                    description = $"One practical learner note in {translationLanguage}. Empty string if there is nothing useful to add.",
                    maxLength = 220
                },
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