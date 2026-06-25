using System.Text.Json;
using LanguageReader.Infrastructure.Ai.Execution;
using LanguageReader.Infrastructure.Ai.Models;
using LanguageReader.Infrastructure.Common;
using LanguageReader.Infrastructure.Exceptions;

namespace LanguageReader.Infrastructure.Ai.Operations.Vocabulary;

public sealed class VocabularyExamplesOperation(
    VocabularyExampleGenerationRequest request) : IAiJsonOperation<VocabularyExamplesOperation.Payload>
{
    private const string SchemaName = "vocabulary_example";

    public string OperationName => "Vocabulary example generation";

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
            ExpectedJsonPropertyCount: 2);
    }

    private static IReadOnlyList<AiProviderMessage> BuildMessages(
        VocabularyExampleGenerationRequest request)
    {
        return
        [
            new(
                AiMessageRole.System,
                "Generate one example sentence for a vocabulary entry."),

            new(
                AiMessageRole.User,
                BuildInput(request))
        ];
    }

    private static string BuildInput(VocabularyExampleGenerationRequest request)
    {
        return $"""
Word: {request.Word.Trim()}
Word language: {request.WordLanguage.Trim()}
Translation language: {request.TranslationLanguage.Trim()}
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
                    description = $"One natural learner-friendly sentence in {wordLanguage} using the word correctly.",
                    minLength = 1,
                    maxLength = 180
                },
                translation = new
                {
                    type = "string",
                    description = $"Translation of the generated sentence in {translationLanguage}.",
                    minLength = 1,
                    maxLength = 220
                }
            }
        };

        return JsonSerializer.Serialize(schema, JsonOptions.Options);
    }

    private static void Validate(VocabularyExampleGenerationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Word))
            throw new ValidationException("Word is required.");

        if (string.IsNullOrWhiteSpace(request.Translation))
            throw new ValidationException("Translation is required.");

        if (string.IsNullOrWhiteSpace(request.WordLanguage))
            throw new ValidationException("Word language is required.");

        if (string.IsNullOrWhiteSpace(request.TranslationLanguage))
            throw new ValidationException("Translation language is required.");
    }

    public sealed record Payload(
        string Text,
        string Translation);
}
