namespace LanguageReader.Infrastructure.Features.Common.Language;

public sealed class VocabularyNormalizationRuleProvider : IVocabularyNormalizationRuleProvider
{
    public VocabularyNormalizationRules GetRules(string? sourceLanguage)
    {
        var normalizedLanguage = SupportedLanguages.Normalize(sourceLanguage);
        return new VocabularyNormalizationRules(
            normalizedLanguage,
            SupportedLanguages.GetVocabularyNormalizationRules(normalizedLanguage));
    }
}
