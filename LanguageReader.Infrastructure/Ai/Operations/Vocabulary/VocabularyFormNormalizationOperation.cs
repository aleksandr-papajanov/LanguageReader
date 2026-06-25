using System.Text.Json;
using LanguageReader.Infrastructure.Ai.Execution;
using LanguageReader.Infrastructure.Ai.Models;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Common.Language;

namespace LanguageReader.Infrastructure.Ai.Operations.Vocabulary;

public sealed class VocabularyFormNormalizationOperation(
    VocabularyNormalizationRequest request,
    VocabularyNormalizationRules normalizationRules) : IAiJsonOperation<VocabularyFormNormalizationOperation.Payload>
{
    private const string SchemaName = "vocabulary_form_normalization";

    public string OperationName => "Vocabulary form normalization";

    public AiJsonOperationRequest BuildRequest()
    {
        Validate(request);

        return new AiJsonOperationRequest(
            OperationName,
            BuildMessages(request, normalizationRules),
            SchemaName: SchemaName,
            JsonSchema: BuildSchema(request),
            request.Text.Length + request.Translation.Length,
            request.ContextSentence?.Length ?? 0,
            ExpectedJsonPropertyCount: 2);
    }

    private static IReadOnlyList<AiProviderMessage> BuildMessages(
        VocabularyNormalizationRequest request,
        VocabularyNormalizationRules normalizationRules)
    {
        return
        [
            new(
                AiMessageRole.System,
                "Return dictionary form and part of speech for one lexical unit."),

            new(
                AiMessageRole.User,
                BuildInput(request, normalizationRules))
        ];
    }

    private static string BuildInput(
        VocabularyNormalizationRequest request,
        VocabularyNormalizationRules normalizationRules)
    {
        var context = string.IsNullOrWhiteSpace(request.ContextSentence)
            ? "none"
            : request.ContextSentence.Trim();

        return $"""
Lexical unit: {request.Text.Trim()}
Language: {request.SourceLanguage.Trim()}
Context: {context}

Dictionary form rules:
{normalizationRules.DictionaryFormInstruction}
""";
    }

    private static string BuildSchema(VocabularyNormalizationRequest request)
    {
        var sourceLanguage = request.SourceLanguage.Trim();

        var schema = new
        {
            type = "object",
            additionalProperties = false,
            required = new[]
            {
                "dictionaryForm",
                "partOfSpeech"
            },
            properties = new
            {
                dictionaryForm = new
                {
                    type = "string",
                    description = $"Dictionary form in {sourceLanguage}.",
                    minLength = 1,
                    maxLength = 120
                },
                partOfSpeech = new
                {
                    type = "string",
                    description = "Short English part-of-speech label: noun, verb, adjective, adverb, phrase, preposition, conjunction, pronoun, interjection, etc.",
                    minLength = 1,
                    maxLength = 24
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

        if (string.IsNullOrWhiteSpace(request.SourceLanguage))
        {
            throw new ValidationException("Source language is required.");
        }
    }

    public sealed record Payload(
        string DictionaryForm,
        string PartOfSpeech);
}
