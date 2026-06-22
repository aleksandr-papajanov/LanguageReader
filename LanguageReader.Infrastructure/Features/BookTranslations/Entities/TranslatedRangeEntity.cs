using LanguageReader.Infrastructure.Features.Ai.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;

namespace LanguageReader.Infrastructure.Features.BookTranslations.Entities;

/// <summary>
/// Persisted translated range for a user and reading item.
/// </summary>
public sealed class TranslatedRangeEntity
{
    /// <summary>
    /// Unique range identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Temporary username.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Source reading item identifier.
    /// </summary>
    public Guid ReadingItemId { get; set; }

    /// <summary>
    /// Paragraph index in parsed book content.
    /// </summary>
    public int ParagraphIndex { get; set; }

    /// <summary>
    /// Start character offset inside the paragraph.
    /// </summary>
    public int StartOffset { get; set; }

    /// <summary>
    /// End character offset inside the paragraph.
    /// </summary>
    public int EndOffset { get; set; }

    /// <summary>
    /// Original selected text.
    /// </summary>
    public string OriginalText { get; set; } = string.Empty;

    /// <summary>
    /// Translated text displayed in place.
    /// </summary>
    public string TranslatedText { get; set; } = string.Empty;

    /// <summary>
    /// Linked vocabulary entry identifier when this translation is saved to vocabulary.
    /// </summary>
    public Guid? VocabularyEntryId { get; set; }

    /// <summary>
    /// Indicates whether the reader should currently show the original text.
    /// </summary>
    public bool ShowOriginal { get; set; }

    /// <summary>
    /// Semantic kind used by backend storage.
    /// </summary>
    public SavedTextKind Kind { get; set; } = SavedTextKind.LexicalUnit;

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// Associated reading item navigation property.
    /// </summary>
    public ReadingItemEntity? ReadingItem { get; set; }

    /// <summary>
    /// Linked vocabulary entry navigation property.
    /// </summary>
    public VocabularyEntryEntity? VocabularyEntry { get; set; }

    /// <summary>
    /// AI operations associated with this translated range.
    /// </summary>
    public ICollection<AiOperationEntity> AiOperations { get; set; } = [];
}

