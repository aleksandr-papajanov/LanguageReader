namespace LanguageReader.Infrastructure.Features.Translation.Services;

internal static class TranslationMappingExtensions
{
    public static TranslationResultDto ToTranslationResultDto(
        this TranslateRequest request,
        string translatedText,
        SelectionKind resolvedSelectionKind,
        AiOperationUsageDto usage)
    {
        return new TranslationResultDto(
            request.SourceText.Trim(),
            translatedText.Trim(),
            resolvedSelectionKind,
            usage);
    }
}
