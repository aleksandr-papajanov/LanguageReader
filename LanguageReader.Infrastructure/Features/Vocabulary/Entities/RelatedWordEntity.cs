namespace LanguageReader.Infrastructure.Features.Vocabulary.Entities;

/// <summary>
/// Related word connection for a vocabulary entry.
/// </summary>
public sealed class RelatedWordEntity
{
    public Guid Id { get; set; }

    public Guid VocabularyEntryId { get; set; }

    public string Word { get; set; } = string.Empty;

    public RelatedWordType Type { get; set; } = RelatedWordType.Related;

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public VocabularyEntryEntity? VocabularyEntry { get; set; }
}
