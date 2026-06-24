using LanguageReader.Infrastructure.Features.ReadingItems.Parsing.Models;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Parsing;

/// <summary>
/// Parses uploaded reading item source files into normalized content blocks.
/// </summary>
public interface IReadingItemContentParser
{
    /// <summary>
    /// Parses a FictionBook 2 XML stream into readable pages.
    /// </summary>
    /// <param name="content">The source file content.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Parsed reading item content.</returns>
    Task<ParsedReadingDocument> ParseAsync(Stream content, CancellationToken cancellationToken = default);
}