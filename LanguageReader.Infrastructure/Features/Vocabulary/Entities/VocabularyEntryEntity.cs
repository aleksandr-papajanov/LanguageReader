using LanguageReader.Infrastructure.Features.Ai.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using System.ComponentModel.DataAnnotations.Schema;


namespace LanguageReader.Infrastructure.Features.Vocabulary.Entities;

/// <summary>
/// Persisted vocabulary item.
/// </summary>
public sealed class VocabularyEntryEntity
{
    /// <summary>
    /// Unique entry identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Temporary username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Original saved word or phrase.
    /// </summary>
    public string Word { get; set; } = string.Empty;

    /// <summary>
    /// Translation text.
    /// </summary>
    public string Translation { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the entry is shown in the user's vocabulary library.
    /// </summary>
    public bool IsVisibleInVocabulary { get; set; } = true;

    /// <summary>
    /// Optional source language.
    /// </summary>
    public string? SourceLanguage { get; set; }

    /// <summary>
    /// Target language.
    /// </summary>
    public string TargetLanguage { get; set; } = string.Empty;

    /// <summary>
    /// Source reading item identifier.
    /// </summary>
    public Guid ReadingItemId { get; set; }

    [NotMapped]
    public Guid BookId
    {
        get => ReadingItemId;
        set => ReadingItemId = value;
    }

    /// <summary>
    /// Paragraph index for returning to the source.
    /// </summary>
    public int ParagraphIndex { get; set; }

    /// <summary>
    /// Character offset for returning to the source.
    /// </summary>
    public int CharacterOffset { get; set; }

    /// <summary>
    /// Type of source fragment saved to vocabulary.
    /// </summary>
    public SelectionKind SelectionKind { get; set; } = SelectionKind.Word;

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Associated reading item navigation property.
    /// </summary>
    public ReadingItemEntity? Book { get; set; }

    [NotMapped]
    public ReadingItemEntity? ReadingItem
    {
        get => Book;
        set => Book = value;
    }

    /// <summary>
    /// Word-specific enrichment details.
    /// </summary>
    public VocabularyWordDetailsEntity? WordDetails { get; set; }

    /// <summary>
    /// Saved example usages for this vocabulary entry.
    /// </summary>
    public ICollection<VocabularyExampleEntity> Examples { get; set; } = [];

    /// <summary>
    /// Saved related words for this vocabulary entry.
    /// </summary>
    public ICollection<RelatedWordEntity> RelatedWords { get; set; } = [];

    /// <summary>
    /// AI operations associated with this vocabulary entry.
    /// </summary>
    public ICollection<AiOperationEntity> AiOperations { get; set; } = [];
}

