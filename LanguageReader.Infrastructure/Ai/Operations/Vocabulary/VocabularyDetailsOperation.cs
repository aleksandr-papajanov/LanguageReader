using System.Text.Json;
using LanguageReader.Infrastructure.Ai.Execution;
using LanguageReader.Infrastructure.Ai.Models;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;

namespace LanguageReader.Infrastructure.Ai.Operations.Vocabulary;

public sealed class VocabularyDetailsOperation(
    VocabularyAutofillRequest request) : IAiJsonOperation<VocabularyDetailsOperation.Payload>
{
    private const string SchemaName = "vocabulary_details";

    public string OperationName => "Vocabulary details";

    public AiJsonOperationRequest BuildRequest()
    {
        Validate(request);

        return new AiJsonOperationRequest(
            OperationName,
            BuildMessages(request),
            SchemaName: SchemaName,
            JsonSchema: BuildSchema(request),
            request.Word.Length + request.Translation.Length,
            request.ContextSentence?.Length ?? 0,
            ExpectedJsonPropertyCount: 9);
    }

    private static IReadOnlyList<AiProviderMessage> BuildMessages(VocabularyAutofillRequest request)
    {
        return
        [
            new(
                AiMessageRole.System,
                "Create concise vocabulary details for a language learner."),

            new(
                AiMessageRole.User,
                BuildInput(request))
        ];
    }

    private static string BuildInput(VocabularyAutofillRequest request)
    {
        var context = string.IsNullOrWhiteSpace(request.ContextSentence)
            ? "none"
            : request.ContextSentence.Trim();

        return $"""
Word: {request.Word.Trim()}
Word language: {request.WordLanguage.Trim()}
Known translation: {request.Translation.Trim()}
Translation language: {request.TranslationLanguage.Trim()}
Context: {context}

Rules:
- Word is already dictionary form.
- Part of speech is already known when available.
- Use context and known translation only for nuance.
- For notes, write only something genuinely useful or interesting.
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
                    description = $"Optional interesting learner note in {translationLanguage}: word formation, root, prefix/suffix, compound structure, usage pattern, false friend, register, or memorable nuance. Use empty string if there is no genuinely useful or interesting note. Do not write generic advice.",
                    maxLength = 220
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

    public sealed record Payload(
        string PrimaryTranslation,
        string Description,
        IReadOnlyList<string> AlternativeTranslations,
        int FrequencyScore,
        string Notes,
        IReadOnlyList<string> Synonyms,
        IReadOnlyList<string> Antonyms,
        IReadOnlyList<string> Related);
}
