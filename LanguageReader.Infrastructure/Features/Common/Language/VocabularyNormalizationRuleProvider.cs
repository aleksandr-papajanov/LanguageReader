namespace LanguageReader.Infrastructure.Features.Common.Language;

public sealed class VocabularyNormalizationRuleProvider : IVocabularyNormalizationRuleProvider
{
    private const string GenericLanguage = "the source language";

    public VocabularyNormalizationRules GetRules(string? sourceLanguage)
    {
        var languageName = LanguageNameNormalizer.Normalize(sourceLanguage, GenericLanguage);
        return new VocabularyNormalizationRules(
            languageName,
            ResolveDictionaryFormInstruction(sourceLanguage, languageName));
    }

    private static string ResolveDictionaryFormInstruction(string? sourceLanguage, string languageName)
    {
        if (MatchesLanguage(sourceLanguage, languageName, "swedish", "sv", "swe", "svenska"))
        {
            return """
For Swedish learner dictionary form:
- Nouns: use indefinite singular with article, e.g. "en bok", "ett hus".
- Verbs: use infinitive with "att", e.g. "att läsa".
- Particle verbs: keep the particle when it changes the meaning, e.g. "att tycka om", "att komma ihåg".
- Reflexive verbs: keep the reflexive pronoun when required, e.g. "att känna sig".
- Adjectives: use common gender singular indefinite form, e.g. "stor".
- Compounds: keep the compound as one lexical item when it is normally written together.
- Multi-word expressions: keep words together when they form one meaning.
""";
        }

        if (MatchesLanguage(sourceLanguage, languageName, "english", "en", "eng"))
        {
            return """
For English learner dictionary form:
- Nouns: use singular form, e.g. "book".
- Verbs: use bare infinitive without "to", e.g. "read".
- Phrasal verbs: keep the particle when it changes the meaning, e.g. "look up", "give up".
- Reflexive/idiomatic verbs: keep required words when they are part of the expression.
- Adjectives/adverbs: use base form, e.g. "big", "quickly".
- Fixed expressions: keep multi-word expressions together when they form one meaning.
""";
        }

        if (MatchesLanguage(sourceLanguage, languageName, "russian", "ru", "rus", "русский", "russkij"))
        {
            return  """
For Russian learner dictionary form:
- Nouns: use nominative singular, e.g. "книга".
- Verbs: use infinitive, e.g. "читать".
- Reflexive verbs: keep "-ся/-сь", e.g. "учиться".
- Aspect pairs: return the form that matches the meaning in context; include the pair only if needed.
- Adjectives: use masculine nominative singular, e.g. "большой".
- Adverbs: use the standard adverb form, e.g. "быстро".
- Fixed expressions: keep words together when they form one lexical meaning.
""";
        }

        return """
For learner dictionary form:
- Return the canonical dictionary entry a learner would expect to save.
- Preserve the meaning from the context, not necessarily the exact inflected form.
- Keep phrasal verbs, particle verbs, reflexive verbs and fixed expressions together when they form one meaning.
- Use the standard lemma/dictionary form for the language.
- Do not return an inflected form unless it is the natural dictionary form.
""";
    }

    private static bool MatchesLanguage(string? sourceLanguage, string languageName, params string[] aliases)
    {
        var values = new[] { sourceLanguage, languageName };
        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim().ToLowerInvariant())
            .Any(value => aliases.Contains(value));
    }
}
