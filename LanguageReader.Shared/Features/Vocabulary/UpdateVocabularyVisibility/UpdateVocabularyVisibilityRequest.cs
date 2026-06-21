namespace LanguageReader.Shared.Features.Vocabulary;

public sealed record UpdateVocabularyVisibilityRequest(
    Guid VocabularyId,
    string Username,
    bool IsVisibleInVocabulary);

public sealed record UpdateVocabularyVisibilityRequestRoute(
    Guid VocabularyId);

public sealed record UpdateVocabularyVisibilityRequestBody(
    string Username,
    bool IsVisibleInVocabulary);
