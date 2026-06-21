namespace LanguageReader.Shared.Features.Vocabulary;

public sealed record DeleteVocabularyEntryRequest(
    Guid VocabularyId,
    string Username);
