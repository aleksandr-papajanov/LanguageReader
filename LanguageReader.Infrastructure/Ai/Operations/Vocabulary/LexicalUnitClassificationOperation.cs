using System.Text.Json;
using LanguageReader.Infrastructure.Ai.Execution;
using LanguageReader.Infrastructure.Ai.Models;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;

namespace LanguageReader.Infrastructure.Ai.Operations.Vocabulary;

public sealed class LexicalUnitClassificationOperation(
    VocabularyNormalizationRequest request) : IAiJsonOperation<LexicalUnitClassificationOperation.Payload>
{
    private const string SchemaName = "lexical_unit_classification";

    public string OperationName => "Lexical unit classification";

    public AiJsonOperationRequest BuildRequest()
    {
        Validate(request);

        return new AiJsonOperationRequest(
            OperationName,
            BuildMessages(request),
            SchemaName: SchemaName,
            JsonSchema: BuildSchema(),
            request.Text.Length + request.Translation.Length,
            request.ContextSentence?.Length ?? 0,
            ExpectedJsonPropertyCount: 1);
    }

    private static IReadOnlyList<AiProviderMessage> BuildMessages(VocabularyNormalizationRequest request)
    {
        return
        [
            new(
                AiMessageRole.System,
                "Decide if selected text is one learnable lexical unit."),

            new(
                AiMessageRole.User,
                BuildInput(request))
        ];
    }

    private static string BuildInput(VocabularyNormalizationRequest request)
    {
        var context = string.IsNullOrWhiteSpace(request.ContextSentence)
            ? "none"
            : request.ContextSentence.Trim();

        return $"""
Selected text: {request.Text.Trim()}
Language: {request.SourceLanguage.Trim()}
Context: {context}

Lexical unit: one word, compound word, fixed expression, phrasal verb, idiom, named term, or short collocation with one meaning.
Not lexical unit: sentence, clause, paragraph, quote, random fragment, or multiple independent words.
""";
    }

    private static string BuildSchema()
    {
        var schema = new
        {
            type = "object",
            additionalProperties = false,
            required = new[] { "isLexicalUnit" },
            properties = new
            {
                isLexicalUnit = new
                {
                    type = "boolean",
                    description = "True if selected text is one learnable vocabulary item."
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

    public sealed record Payload(bool IsLexicalUnit);
}
