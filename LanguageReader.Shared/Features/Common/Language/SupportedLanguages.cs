namespace LanguageReader.Shared.Features.Common;

public static class SupportedLanguages
{
    public const string Russian = "Russian";
    public const string Swedish = "Swedish";
    public const string English = "English";

    public static readonly IReadOnlyList<SupportedLanguage> All =
    [
        new(
            Russian,
            "ru",
            "Nouns should use nominative singular when appropriate. Verbs should use infinitive. Adjectives should use masculine nominative singular when appropriate. Preserve the dictionary form a Russian learner expects."),
        new(
            Swedish,
            "sv",
            "Nouns should include the appropriate article or form when relevant. Verbs should use the dictionary infinitive form. Adjectives should use an appropriate base form. Prefer the form a Swedish learner expects in a dictionary."),
        new(
            English,
            "en",
            "Verbs should use the base infinitive form. Nouns should use singular form unless the plural is the natural dictionary form. Adjectives should use base form.")
    ];

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return English;
        }

        var normalized = value.Trim();
        var match = All.FirstOrDefault(language =>
            string.Equals(language.Name, normalized, StringComparison.OrdinalIgnoreCase)
            || string.Equals(language.Code, normalized, StringComparison.OrdinalIgnoreCase));

        return match?.Name ?? English;
    }

    public static string GetVocabularyNormalizationRules(string? value)
    {
        var normalized = Normalize(value);
        return All.First(language => language.Name == normalized).VocabularyNormalizationRules;
    }
}
