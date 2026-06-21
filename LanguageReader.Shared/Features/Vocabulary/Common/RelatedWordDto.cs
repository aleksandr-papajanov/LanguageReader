namespace LanguageReader.Shared.Features.Vocabulary;

public sealed record RelatedWordDto(
    Guid Id,
    string Word,
    RelatedWordType Type);
