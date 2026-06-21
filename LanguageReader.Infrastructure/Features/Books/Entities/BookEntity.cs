namespace LanguageReader.Infrastructure.Features.Books.Entities;

/// <summary>
/// Stored book metadata.
/// </summary>
public sealed class BookEntity
{
    /// <summary>
    /// Unique book identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Temporary owner username.
    /// </summary>
    public string OwnerUsername { get; set; } = string.Empty;

    /// <summary>
    /// Display title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Name of the uploaded source file.
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// Original language of the book text.
    /// </summary>
    public string OriginalLanguage { get; set; } = "Unknown";

    /// <summary>
    /// Storage-relative path for the uploaded source file.
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the book is visible in the public library.
    /// </summary>
    public bool IsPublic { get; set; }

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }
}

