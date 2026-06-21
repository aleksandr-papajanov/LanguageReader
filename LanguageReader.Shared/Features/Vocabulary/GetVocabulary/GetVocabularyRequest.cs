namespace LanguageReader.Shared.Features.Vocabulary;

public sealed record GetVocabularyRequest(
    string Username,
    bool? IncludeHidden);

