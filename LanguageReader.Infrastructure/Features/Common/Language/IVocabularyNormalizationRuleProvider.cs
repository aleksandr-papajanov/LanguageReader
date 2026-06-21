namespace LanguageReader.Infrastructure.Features.Common.Language;

public interface IVocabularyNormalizationRuleProvider
{
    VocabularyNormalizationRules GetRules(string? sourceLanguage);
}
