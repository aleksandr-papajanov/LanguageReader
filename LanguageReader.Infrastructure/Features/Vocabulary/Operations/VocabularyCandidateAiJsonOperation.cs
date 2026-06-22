using System.Text.Json;
using LanguageReader.Infrastructure.Agents.Json.Models;
using LanguageReader.Infrastructure.Agents.Json.Operations;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Common.Language;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Operations;

internal sealed class VocabularyCandidateAiJsonOperation(
    VocabularyNormalizationRequest request,
    VocabularyNormalizationRules normalizationRules) : IAiJsonOperation<VocabularyCandidateAiJsonOperation.Payload>
{
    private const string OperationName = "Vocabulary candidate";
    private const string SchemaName = "vocabulary_candidate";
    private const string Model = "gpt-5-mini";

    public AiOperationKind Kind => AiOperationKind.VocabularyNormalization;

    public string ProviderName => "OpenAI";

    public AiJsonOperationRequest BuildRequest()
    {
        Validate(request);

        return new AiJsonOperationRequest(
            Kind,
            OperationName,
            BuildInstructions(),
            BuildInput(request, normalizationRules),
            SchemaName: SchemaName,
            JsonSchema: BuildSchema(request),
            Model: Model,
            request.Text.Length + request.Translation.Length,
            request.ContextSentence?.Length ?? 0,
            ExpectedJsonPropertyCount: 2);
    }

    private static string BuildInput(
        VocabularyNormalizationRequest request,
        VocabularyNormalizationRules normalizationRules)
    {
        var context = string.IsNullOrWhiteSpace(request.ContextSentence)
            ? "none"
            : request.ContextSentence.Trim();

        return $"""
Task: decide if selected text is one vocabulary item. If yes, return its dictionary form.

Selected text: {request.Text.Trim()}
Language: {request.SourceLanguage.Trim()}
Context: {context}

Lexical unit = one word, compound word, fixed expression, phrasal verb, idiom, named term, or short collocation with one meaning.
Not lexical unit = sentence, clause, paragraph, quote, random fragment, or multiple independent words.

Dictionary form rules:
{normalizationRules.DictionaryFormInstruction}
""";
    }

    private static string BuildInstructions()
    {
        return """
Classify and normalize vocabulary selections.
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
                "isLexicalUnit",
                "dictionaryForm"
            },
            properties = new
            {
                isLexicalUnit = new
                {
                    type = "boolean",
                    description = "True if selected text is one learnable vocabulary item."
                },
                dictionaryForm = new
                {
                    type = "string",
                    description = $"Dictionary form in {sourceLanguage}. Empty string when isLexicalUnit is false.",
                    minLength = 0,
                    maxLength = 120
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

    internal sealed record Payload(
        bool IsLexicalUnit,
        string DictionaryForm);
}
