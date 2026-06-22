namespace LanguageReader.Shared.Features.Vocabulary;

public sealed record VocabularyEntryDto(
    Guid Id,
    string Word,
    string Translation,
    string SourceLanguage,
    string TargetLanguage,
    string ReadingItemTitle,
    Guid? ReadingItemId,
    string Username,
    ReadingPositionDto? Position,
    SavedTextKind Kind,
    VocabularyWordDetailsDto? WordDetails,
    IReadOnlyList<RelatedWordDto> RelatedWords,
    IReadOnlyList<VocabularyExampleDto> Examples,
    AiUsageSummaryDto UsageSummary,
    IReadOnlyList<AiOperationDto> Operations,
    bool IsVisibleInVocabulary,
    DateTimeOffset CreatedAtUtc);
