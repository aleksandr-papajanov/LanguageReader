namespace LanguageReader.Shared.Features.Translation;

public sealed record TranslationResultDto(
    string SourceText,
    string Translation,
    AiOperationUsageDto Usage);

