namespace LanguageReader.Shared.Features.Vocabulary;

public sealed record DeleteVocabularyExampleRequest(
    Guid VocabularyId,
    Guid ExampleId,
    string Username);
