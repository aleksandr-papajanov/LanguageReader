namespace LanguageReader.Shared.Features.Vocabulary;

public sealed record AddVocabularyExampleRequest(
    Guid VocabularyId,
    string Username);

public sealed record AddVocabularyExampleRequestRoute(
    Guid VocabularyId);

public sealed record AddVocabularyExampleRequestBody(
    string Username);