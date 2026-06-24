namespace LanguageReader.Shared.Features.Common;

public sealed record SupportedLanguage(
    string Name,
    string Code,
    string VocabularyNormalizationRules);
