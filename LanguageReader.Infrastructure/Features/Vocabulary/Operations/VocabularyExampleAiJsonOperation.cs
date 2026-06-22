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
    private const string OperationName = "Vocabulary example generation";
    private const string SchemaName = "vocabulary_example";
    private const string Model = "gpt-5-mini";

    public AiOperationKind Kind => AiOperationKind.VocabularyExampleGeneration;

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
            ExpectedJsonPropertyCount: 2);
    }

    private static string BuildInput(VocabularyExampleGenerationRequest request)
    {
        return $"""
Task: generate one example sentence for a vocabulary entry.

Word: {request.Word.Trim()}
Word language: {request.WordLanguage.Trim()}
Known translation: {request.Translation.Trim()}
Translation language: {request.TranslationLanguage.Trim()}
""";
    }

    private static string BuildInstructions()
    {
        return """
Create concise learner-friendly vocabulary examples.
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