using LanguageReader.Infrastructure.Features.ReadingItems.Entities;

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

    /// <summary>
    /// Percentage from 0 to 100.
    /// </summary>
    public double ProgressPercent { get; set; }

    /// <summary>
    /// Block index in parsed reading item content.
    /// </summary>
    public int BlockIndex { get; set; }

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
    public ReadingItemEntity? ReadingItem { get; set; }
}

