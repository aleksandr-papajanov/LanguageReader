namespace LanguageReader.Infrastructure.Features.Common.Language;

public static class LanguageNameNormalizer
{
    public static string Normalize(string? value)
    {
        return SupportedLanguages.Normalize(value);
    }
}
