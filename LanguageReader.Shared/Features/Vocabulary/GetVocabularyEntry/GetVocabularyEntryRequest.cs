namespace LanguageReader.Shared.Features.Vocabulary;

public sealed record GetVocabularyEntryRequest(
    Guid VocabularyId,
    string Username);
