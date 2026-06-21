namespace LanguageReader.Shared.Features.Vocabulary;

public sealed record AutofillVocabularyEntryRequest(
    Guid VocabularyId,
    string Username);

public sealed record AutofillVocabularyEntryRequestRoute(
    Guid VocabularyId);

public sealed record AutofillVocabularyEntryRequestBody(
    string Username);