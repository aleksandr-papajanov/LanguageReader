using LanguageReader.Infrastructure.Features.ReadingItems.Entities;

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

    public bool IsFromReadingItem { get; set; }

    public Guid? ReadingItemId { get; set; }

    public int? ParagraphIndex { get; set; }

    public int? CharacterOffset { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public VocabularyEntryEntity? VocabularyEntry { get; set; }

    public ReadingItemEntity? ReadingItem { get; set; }
}
