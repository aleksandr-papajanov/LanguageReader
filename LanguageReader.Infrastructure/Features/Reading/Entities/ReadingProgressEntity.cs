using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace LanguageReader.Infrastructure.Features.Reading.Entities;

/// <summary>
/// Persisted reading progress for a user and reading item.
/// </summary>
public sealed class ReadingProgressEntity
{
    /// <summary>
    /// Unique progress identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Temporary username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Reading item identifier.
    /// </summary>
    public Guid ReadingItemId { get; set; }

    [NotMapped]
    public Guid BookId
    {
        get => ReadingItemId;
        set => ReadingItemId = value;
    }

    /// <summary>
    /// Percentage from 0 to 100.
    /// </summary>
    public double ProgressPercent { get; set; }

    /// <summary>
    /// Paragraph index in parsed book content.
    /// </summary>
    public int ParagraphIndex { get; set; }

    /// <summary>
    /// Character offset inside the paragraph.
    /// </summary>
    public int CharacterOffset { get; set; }

    /// <summary>
    /// Last opened timestamp.
    /// </summary>
    public DateTimeOffset LastOpenedAtUtc { get; set; }

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
}

