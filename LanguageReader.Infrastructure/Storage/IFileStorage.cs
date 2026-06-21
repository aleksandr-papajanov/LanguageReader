namespace LanguageReader.Infrastructure.Storage;

/// <summary>
/// Abstraction for file storage providers.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Saves content at the specified relative path.
    /// </summary>
    /// <param name="relativePath">The storage-relative path.</param>
    /// <param name="content">The content stream to save.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The storage-relative path for the saved content.</returns>
    Task<string> SaveAsync(string relativePath, Stream content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens content from the specified relative path.
    /// </summary>
    /// <param name="relativePath">The storage-relative path.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A readable stream for the stored content.</returns>
    Task<Stream> OpenReadAsync(string relativePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes content at the specified relative path.
    /// </summary>
    /// <param name="relativePath">The storage-relative path.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}

