using System.Text;
using System.Xml.Linq;

namespace LanguageReader.Infrastructure.Features.Books.Parsing;

/// <summary>
/// Minimal FictionBook 2 XML parser for the first reader iteration.
/// </summary>
public sealed class Fb2BookContentParser : IBookContentParser
{
    private const int TargetPageLength = 3500;

    /// <inheritdoc />
    public async Task<ParsedBook> ParseAsync(Stream content, CancellationToken cancellationToken = default)
    {
        var document = await XDocument.LoadAsync(content, LoadOptions.None, cancellationToken);
        var title = document
            .Descendants()
            .FirstOrDefault(element => element.Name.LocalName == "book-title")
            ?.Value
            .Trim();

        var textBlocks = document
            .Descendants()
            .Where(element => element.Name.LocalName is "p" or "subtitle" or "text-author")
            .Select(element => NormalizeWhitespace(element.Value))
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToArray();

        var pages = SplitIntoPages(textBlocks);
        if (pages.Count == 0)
        {
            pages.Add("No readable text was found in this file.");
        }

        return new ParsedBook(string.IsNullOrWhiteSpace(title) ? null : title, pages);
    }

    private static List<string> SplitIntoPages(IEnumerable<string> textBlocks)
    {
        var pages = new List<string>();
        var builder = new StringBuilder();

        foreach (var block in textBlocks)
        {
            if (builder.Length > 0 && builder.Length + block.Length > TargetPageLength)
            {
                pages.Add(builder.ToString().Trim());
                builder.Clear();
            }

            builder.AppendLine(block);
            builder.AppendLine();
        }

        if (builder.Length > 0)
        {
            pages.Add(builder.ToString().Trim());
        }

        return pages;
    }

    private static string NormalizeWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);
        var previousWasWhitespace = false;

        foreach (var character in value)
        {
            if (char.IsWhiteSpace(character))
            {
                if (!previousWasWhitespace)
                {
                    builder.Append(' ');
                    previousWasWhitespace = true;
                }

                continue;
            }

            builder.Append(character);
            previousWasWhitespace = false;
        }

        return builder.ToString().Trim();
    }
}

