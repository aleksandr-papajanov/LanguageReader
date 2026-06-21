namespace LanguageReader.Infrastructure.Features.Vocabulary.Models.Enrichment;

public sealed record VocabularyRelatedWordSeed(
    string Word,
    RelatedWordType Type);
