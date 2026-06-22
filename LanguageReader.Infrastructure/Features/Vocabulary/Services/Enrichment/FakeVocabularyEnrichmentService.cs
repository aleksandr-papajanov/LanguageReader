using Bogus;
using LanguageReader.Infrastructure.Features.Ai.Models;
using LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;
using LanguageReader.Shared.Features.Settings;
using ValidationException = LanguageReader.Infrastructure.Exceptions.ValidationException;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Services.Enrichment;

/// <summary>
/// Local fake vocabulary backend that generates deterministic placeholder enrichment.
/// </summary>
public sealed class FakeVocabularyEnrichmentService : IVocabularyEnrichmentBackend
{
    private static readonly string[] PartOfSpeechValues = ["noun", "verb", "adjective", "adverb"];
    private static readonly string[] CyrillicLexicon =
    [
        "образ",
        "движение",
        "привычка",
        "тишина",
        "оттенок",
        "решение",
        "намерение",
        "признак",
        "разговор",
        "поворот",
        "деталь",
        "случай"
    ];
    private static readonly string[] LatinLexicon =
    [
        "nyans",
        "riktning",
        "vana",
        "tystnad",
        "avsikt",
        "tecken",
        "samtal",
        "vinkel",
        "detalj",
        "uttryck",
        "betydelse",
        "sammanhang"
    ];

    public AiServiceMode Mode => AiServiceMode.Fake;

    public Task<VocabularyNormalizationResult> NormalizeAsync(
        VocabularyNormalizationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validate(request.Text, request.Translation, request.SourceLanguage, request.TargetLanguage);

        var isLexicalUnit = IsShortLexicalCandidate(request.Text);
        var dictionaryForm = isLexicalUnit ? BuildDictionaryForm(request.Text) : string.Empty;
        var pricing = AiPricingCatalog.GetPricing("FakeAI", "fake-bogus-v2");
        var usage = AiOperationUsageFactory.Create(
            AiOperationKind.VocabularyNormalization,
            "FakeAI",
            "fake-bogus-v2",
            BuildInput(request.Text, request.Translation, request.SourceLanguage, request.TargetLanguage, request.ContextSentence),
            dictionaryForm,
            pricing.InputUsdPerMillionTokens,
            pricing.OutputUsdPerMillionTokens);

        return Task.FromResult(new VocabularyNormalizationResult(isLexicalUnit, dictionaryForm, usage));
    }

    public Task<VocabularyAutofillResult> AutofillAsync(
        VocabularyAutofillRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validate(request.Word, request.Translation, request.WordLanguage, request.TranslationLanguage);

        var faker = CreateFaker(request.WordLanguage, request.Word);
        var normalizedWord = request.Word.Trim();
        var normalizedTranslation = request.Translation.Trim();
        var sourceLexicon = GetLexicon(normalizedWord);
        var translationLexicon = GetLexicon(normalizedTranslation);
        var description = BuildDescription(normalizedWord, normalizedTranslation, request.ContextSentence, faker, sourceLexicon);

        var relatedWords = BuildRelatedWords(faker, sourceLexicon).ToList();
        var alternativeTranslations = Enumerable.Range(0, 3)
            .Select(index => index == 0
                ? normalizedTranslation
                : $"{faker.PickRandom(translationLexicon)} {faker.PickRandom(translationLexicon)}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var notes = string.IsNullOrWhiteSpace(request.ContextSentence)
            ? BuildStandaloneNote(normalizedTranslation, faker, translationLexicon)
            : BuildContextualNote(faker, translationLexicon);

        var outputText = string.Join(
            ' ',
            [
                description,
                string.Join(' ', alternativeTranslations),
                string.Join(' ', relatedWords.Select(item => item.Word)),
                notes
            ]);

        var pricing = AiPricingCatalog.GetPricing("FakeAI", "fake-bogus-v2");
        var usage = AiOperationUsageFactory.Create(
            AiOperationKind.VocabularyAutofill,
            "FakeAI",
            "fake-bogus-v2",
            BuildInput(request.Word, request.Translation, request.WordLanguage, request.TranslationLanguage, request.ContextSentence),
            outputText,
            pricing.InputUsdPerMillionTokens,
            pricing.OutputUsdPerMillionTokens);

        return Task.FromResult(new VocabularyAutofillResult(
            PrimaryTranslation: normalizedTranslation,
            Description: description,
            FrequencyScore: faker.Random.Int(42, 94),
            AlternativeTranslations: alternativeTranslations,
            RelatedWords: relatedWords,
            Usage: usage,
            PartOfSpeech: faker.PickRandom(PartOfSpeechValues),
            Notes: notes));
    }

    public Task<VocabularyGeneratedExampleResult> GenerateExampleAsync(
        VocabularyExampleGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validate(request.Word, request.Translation, request.WordLanguage, request.TranslationLanguage);

        var faker = CreateFaker(request.WordLanguage, request.Word + "::example");
        var normalizedWord = request.Word.Trim();
        var text = BuildExampleSentence(normalizedWord, request.ContextSentence, faker);
        var translation = BuildExampleTranslation(request.Translation.Trim(), faker);

        var pricing = AiPricingCatalog.GetPricing("FakeAI", "fake-bogus-v2");
        var usage = AiOperationUsageFactory.Create(
            AiOperationKind.VocabularyExampleGeneration,
            "FakeAI",
            "fake-bogus-v2",
            BuildInput(request.Word, request.Translation, request.WordLanguage, request.TranslationLanguage, request.ContextSentence),
            $"{text} {translation}",
            pricing.InputUsdPerMillionTokens,
            pricing.OutputUsdPerMillionTokens);

        return Task.FromResult(new VocabularyGeneratedExampleResult(text, translation, usage));
    }

    private static void Validate(string word, string translation, string wordLanguage, string translationLanguage)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            throw new ValidationException("Word is required.");
        }

        if (string.IsNullOrWhiteSpace(translation))
        {
            throw new ValidationException("Translation is required.");
        }

        if (string.IsNullOrWhiteSpace(wordLanguage))
        {
            throw new ValidationException("Word language is required.");
        }

        if (string.IsNullOrWhiteSpace(translationLanguage))
        {
            throw new ValidationException("Translation language is required.");
        }
    }

    private static Faker CreateFaker(string wordLanguage, string seedText)
    {
        var locale = wordLanguage.Trim().ToLowerInvariant() switch
        {
            var value when value.Contains("russian", StringComparison.Ordinal) || value.Contains("рус", StringComparison.Ordinal) => "ru",
            _ => "en"
        };

        return new Faker(locale)
        {
            Random = new Randomizer(Math.Abs(HashCode.Combine(locale, seedText)))
        };
    }

    private static IEnumerable<VocabularyRelatedWordSeed> BuildRelatedWords(Faker faker, string[] lexicon)
    {
        yield return new VocabularyRelatedWordSeed($"{faker.PickRandom(lexicon)} {faker.PickRandom(lexicon)}", RelatedWordType.Synonym);
        yield return new VocabularyRelatedWordSeed($"{faker.PickRandom(lexicon)} {faker.PickRandom(lexicon)}", RelatedWordType.Synonym);
        yield return new VocabularyRelatedWordSeed($"{faker.PickRandom(lexicon)} {faker.PickRandom(lexicon)}", RelatedWordType.Antonym);
        yield return new VocabularyRelatedWordSeed($"{faker.PickRandom(lexicon)} {faker.PickRandom(lexicon)}", RelatedWordType.Related);
    }

    private static string BuildDescription(string word, string translation, string? contextSentence, Faker faker, string[] sourceLexicon)
    {
        var flavor = faker.PickRandom(sourceLexicon);

        if (string.IsNullOrWhiteSpace(contextSentence))
        {
            return DetectScriptHint(word) == ScriptHint.Cyrillic
                ? $"«{word}» обычно передает значение «{translation}» и нередко встречается в бытовой речи, диалогах и повествовании."
                : $"\"{word}\" carries the sense of \"{translation}\" and often appears in everyday speech, dialogue, and narrative prose.";
        }

        return DetectScriptHint(word) == ScriptHint.Cyrillic
            ? $"«{word}» в этой фразе ближе к значению «{translation}» и помогает передать {flavor} высказывания без лишней буквальности."
            : $"In this sentence, \"{word}\" is closer to \"{translation}\" and helps express the {flavor} of the statement without sounding too literal.";
    }

    private static string BuildDictionaryForm(string word)
    {
        return word.Trim().ToLowerInvariant();
    }

    private static bool IsShortLexicalCandidate(string text)
    {
        var tokens = text
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Any(char.IsLetter))
            .ToArray();

        return tokens.Length is > 0 and <= 3;
    }

    private static string BuildExampleSentence(string word, string? contextSentence, Faker faker)
    {
        if (!string.IsNullOrWhiteSpace(contextSentence))
        {
            return $"{faker.Lorem.Sentence(4)} {word} {faker.Lorem.Sentence(5).TrimEnd('.')}.";
        }

        return $"{word} {faker.Lorem.Sentence(faker.Random.Int(5, 8)).TrimStart()}";
    }

    private static string BuildExampleTranslation(string translation, Faker faker)
    {
        var lexicon = GetLexicon(translation);
        return DetectScriptHint(translation) == ScriptHint.Cyrillic
            ? $"Пример показывает значение «{translation}» в более естественном употреблении и подчеркивает {faker.PickRandom(lexicon)} фразы."
            : $"This example highlights the meaning of \"{translation}\" in more natural usage and emphasizes the {faker.PickRandom(lexicon)} of the phrase.";
    }

    private static string BuildInput(string word, string translation, string wordLanguage, string translationLanguage, string? contextSentence)
    {
        return string.IsNullOrWhiteSpace(contextSentence)
            ? $"{word} | {translation} | {wordLanguage} | {translationLanguage}"
            : $"{word} | {translation} | {wordLanguage} | {translationLanguage} | {contextSentence.Trim()}";
    }

    private static string BuildStandaloneNote(string translation, Faker faker, string[] lexicon)
    {
        return DetectScriptHint(translation) == ScriptHint.Cyrillic
            ? "Часто встречается в нейтральной речи и коротких текстах."
            : $"Common in neutral usage and short texts, especially when the {faker.PickRandom(lexicon)} is straightforward.";
    }

    private static string BuildContextualNote(Faker faker, string[] lexicon)
    {
        return lexicon == CyrillicLexicon
            ? $"В текущем контексте слово ближе к значению «{faker.PickRandom(lexicon)}» и зависит от соседней фразы."
            : $"In this context, the word leans toward a {faker.PickRandom(lexicon)}-based meaning and depends on the surrounding phrase.";
    }

    private static string[] GetLexicon(string text)
    {
        return DetectScriptHint(text) == ScriptHint.Cyrillic
            ? CyrillicLexicon
            : LatinLexicon;
    }

    private static ScriptHint DetectScriptHint(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return ScriptHint.Latin;
        }

        foreach (var character in text)
        {
            if (character is >= '\u0400' and <= '\u04FF')
            {
                return ScriptHint.Cyrillic;
            }
        }

        return ScriptHint.Latin;
    }

    private enum ScriptHint
    {
        Latin,
        Cyrillic
    }
}
