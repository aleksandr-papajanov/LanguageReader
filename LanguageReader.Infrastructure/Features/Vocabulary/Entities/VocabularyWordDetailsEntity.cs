namespace LanguageReader.Infrastructure.Features.Vocabulary.Entities;

/// <summary>
/// Word-only enrichment details linked to a vocabulary entry.
/// </summary>
public sealed class VocabularyWordDetailsEntity
{
    public Guid VocabularyEntryId { get; set; }

    public string? SeenForm { get; set; }

    public string? DictionaryForm { get; set; }

    public string? PartOfSpeech { get; set; }

    public string? Description { get; set; }

    public int? FrequencyScore { get; set; }

    public string? Notes { get; set; }

    public VocabularyEntryEntity? VocabularyEntry { get; set; }
}
