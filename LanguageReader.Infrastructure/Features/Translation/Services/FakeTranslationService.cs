using Bogus;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Ai.Models;
using LanguageReader.Shared.Features.Settings;
using ValidationException = LanguageReader.Infrastructure.Exceptions.ValidationException;

namespace LanguageReader.Infrastructure.Features.Translation.Services;

/// <summary>
/// Local fake translation backend that returns deterministic, more realistic placeholder content.
/// </summary>
public sealed class FakeTranslationService : ITranslationBackend
{
    private static readonly string[] RussianWordMeanings =
    [
        "образ",
        "смысл",
        "значение",
        "оттенок",
        "роль",
        "признак",
        "действие",
        "состояние",
        "явление",
        "подробность"
    ];

    public AiServiceMode Mode => AiServiceMode.Fake;

    public Task<TranslationResultDto> TranslateAsync(
        TranslateRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateRequest(request);

        var sourceText = request.SourceText.Trim();
        var faker = CreateFaker(request.TargetLanguage, sourceText);
        var resolvedSelectionKind = ResolveSelectionKind(request, sourceText);
        var translatedText = resolvedSelectionKind switch
        {
            SelectionKind.Word => GenerateWordTranslation(sourceText, request.OriginalText, faker),
            SelectionKind.Sentence => GenerateSentenceTranslation(faker),
            SelectionKind.Paragraph or SelectionKind.Page => GenerateParagraphTranslation(faker),
            SelectionKind.Phrase => GenerateSentenceTranslation(faker),
            _ => GenerateSentenceTranslation(faker)
        };

        var pricing = AiPricingCatalog.GetPricing("FakeAI", "fake-bogus-v2");
        var usage = AiOperationUsageFactory.Create(
            AiOperationKind.Translation,
            "FakeAI",
            "fake-bogus-v2",
            BuildTranslationInput(sourceText, request.OriginalText, request.SourceLanguage, request.TargetLanguage),
            translatedText,
            pricing.InputUsdPerMillionTokens,
            pricing.OutputUsdPerMillionTokens);

        return Task.FromResult(request.ToTranslationResultDto(
            translatedText,
            resolvedSelectionKind,
            usage));
    }

    private static void ValidateRequest(TranslateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TargetLanguage))
        {
            throw new ValidationException("Select a learning language in Settings before translating.");
        }

        if (string.IsNullOrWhiteSpace(request.SourceText))
        {
            throw new ValidationException("Select text before translating.");
        }
    }

    private static Faker CreateFaker(string targetLanguage, string seedText)
    {
        var locale = targetLanguage.Trim().ToLowerInvariant() switch
        {
            var value when value.Contains("russian", StringComparison.Ordinal) || value.Contains("рус", StringComparison.Ordinal) => "ru",
            _ => "en"
        };

        return new Faker(locale)
        {
            Random = new Randomizer(Math.Abs(HashCode.Combine(locale, seedText)))
        };
    }

    private static string GenerateWordTranslation(string word, string? contextSentence, Faker faker)
    {
        var baseMeaning = faker.PickRandom(RussianWordMeanings);
        if (string.IsNullOrWhiteSpace(contextSentence))
        {
            return $"{baseMeaning}, {faker.PickRandom(RussianWordMeanings)}";
        }

        return $"{baseMeaning} в этом контексте, {faker.PickRandom(RussianWordMeanings)} по смыслу";
    }

    private static string GenerateSentenceTranslation(Faker faker)
    {
        return faker.Lorem.Sentence(faker.Random.Int(7, 12));
    }

    private static string GenerateParagraphTranslation(Faker faker)
    {
        return string.Join(" ", Enumerable.Range(0, faker.Random.Int(2, 4))
            .Select(_ => faker.Lorem.Sentence(faker.Random.Int(8, 14))));
    }

    private static SelectionKind ResolveSelectionKind(TranslateRequest request, string sourceText)
    {
        if (request.SelectionKind != SelectionKind.Phrase)
        {
            return request.SelectionKind;
        }

        var tokens = sourceText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return tokens.Length switch
        {
            0 => SelectionKind.Phrase,
            <= 3 when tokens.All(token => token.Any(char.IsLetter)) => SelectionKind.Word,
            _ => SelectionKind.Phrase
        };
    }

    private static string BuildTranslationInput(string sourceText, string? contextSentence, string? sourceLanguage, string targetLanguage)
    {
        return string.IsNullOrWhiteSpace(contextSentence)
            ? $"{sourceText}\nSource language: {sourceLanguage ?? "Unknown"}\nTarget language: {targetLanguage}"
            : $"{sourceText}\nSource language: {sourceLanguage ?? "Unknown"}\nTarget language: {targetLanguage}\nContext: {contextSentence.Trim()}";
    }
}
