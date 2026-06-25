namespace LanguageReader.Infrastructure.Features.Translation.Mapping;

internal static class TranslationMappingExtensions
{
    public static TranslationResultDto ToTranslationResultDto(
        this TranslateRequest request,
        string translatedText,
        AiOperationUsageDto usage)
    {
        return new TranslationResultDto(
            request.SourceText.Trim(),
            translatedText.Trim(),
            usage);
    }
}
