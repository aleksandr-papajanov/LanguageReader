namespace LanguageReader.Shared.Features.Vocabulary;

public sealed record VocabularyWordDetailsDto(
    string? SeenForm,
    string? DictionaryForm,
    string? PartOfSpeech,
    string? Description,
    int? FrequencyScore,
    string? Notes);
