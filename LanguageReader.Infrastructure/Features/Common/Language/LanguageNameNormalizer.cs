using System.Globalization;

namespace LanguageReader.Infrastructure.Features.Common.Language;

public static class LanguageNameNormalizer
{
    public static string Normalize(string? value, string fallback = "Unknown")
    {
        return NormalizeOrNull(value) ?? fallback;
    }

    public static string? NormalizeOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var candidate = value.Trim();
        foreach (var segment in candidate.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var token = segment.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
            var normalized = TryNormalizeLanguageTag(token);
            if (normalized is not null)
            {
                return normalized;
            }
        }

        return TryNormalizeLanguageTag(candidate) ?? ToTitleCase(candidate);
    }

    private static string? TryNormalizeLanguageTag(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().Replace('_', '-');
        var primaryLanguage = normalized.Split('-', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];

        if (TryMapThreeLetterCode(primaryLanguage, out var mappedLanguage))
        {
            return mappedLanguage;
        }

        try
        {
            return CultureInfo.GetCultureInfo(primaryLanguage).EnglishName;
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }

    private static bool TryMapThreeLetterCode(string code, out string language)
    {
        language = code.ToLowerInvariant() switch
        {
            "eng" => "English",
            "rus" => "Russian",
            "swe" => "Swedish",
            _ => string.Empty
        };

        return !string.IsNullOrWhiteSpace(language);
    }

    private static string ToTitleCase(string value)
    {
        var textInfo = CultureInfo.InvariantCulture.TextInfo;
        return textInfo.ToTitleCase(value.Trim().ToLowerInvariant());
    }
}
