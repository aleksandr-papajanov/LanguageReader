using LanguageReader.Infrastructure.Features.Books.Parsing.Models;

namespace LanguageReader.Infrastructure.Features.Books.Parsing;

/// <summary>
/// Parses uploaded book files into readable pages.
/// </summary>
public interface IBookContentParser
{
    /// <summary>
    /// Parses a FictionBook 2 XML stream into readable pages.
    /// </summary>
    /// <param name="content">The source file content.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>Parsed book content.</returns>
    Task<ParsedBook> ParseAsync(Stream content, CancellationToken cancellationToken = default);
}