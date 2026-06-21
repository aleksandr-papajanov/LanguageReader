using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Entities;

/// <summary>
/// Example usage saved for a vocabulary entry.
/// </summary>
public sealed class VocabularyExampleEntity
{
    public Guid Id { get; set; }

    public Guid VocabularyEntryId { get; set; }

    public string Text { get; set; } = string.Empty;

    public string? Translation { get; set; }

    public bool IsFromBook { get; set; }

    public Guid? ReadingItemId { get; set; }

    [NotMapped]
    public Guid? BookId
    {
        get => ReadingItemId;
        set => ReadingItemId = value;
    }

    public int? ParagraphIndex { get; set; }

    public int? CharacterOffset { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public VocabularyEntryEntity? VocabularyEntry { get; set; }

    public ReadingItemEntity? Book { get; set; }

    [NotMapped]
    public ReadingItemEntity? ReadingItem
    {
        get => Book;
        set => Book = value;
    }
}
