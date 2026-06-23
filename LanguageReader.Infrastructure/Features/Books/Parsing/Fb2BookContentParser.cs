using System.Text;
using System.Xml.Linq;
using LanguageReader.Infrastructure.Features.Books.Parsing.Models;
using LanguageReader.Shared.Features.ReadingItems;

namespace LanguageReader.Infrastructure.Features.Books.Parsing;

public sealed class Fb2BookContentParser : IBookContentParser
{
    private static readonly XNamespace XLink = "http://www.w3.org/1999/xlink";

    public async Task<ParsedBook> ParseAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var document = await XDocument.LoadAsync(
            content,
            LoadOptions.PreserveWhitespace,
            cancellationToken);

        var title = GetBookTitle(document);
        var images = ParseImages(document);

        var bodies = document
            .Root?
            .Elements()
            .Where(e => e.Name.LocalName == "body")
            .Where(e => e.Attribute("name")?.Value != "notes")
            .ToList() ?? [];

        var blocks = new List<ParsedBookBlock>();

        foreach (var body in bodies)
        {
            var bodyName = body.Attribute("name")?.Value;

            if (!string.IsNullOrWhiteSpace(bodyName))
            {
                AddTextBlock(blocks, ReadingContentBlockType.Heading1, bodyName);
            }

            ParseContainer(body, blocks);
        }

        TrimEmptyLines(blocks);

        if (blocks.Count == 0)
        {
            blocks.Add(new ParsedBookBlock(
                ReadingContentBlockType.Paragraph,
                "No readable text was found in this file."));
        }

        return new ParsedBook(
            string.IsNullOrWhiteSpace(title) ? null : title,
            blocks,
            images);
    }

    private static string? GetBookTitle(XDocument document)
    {
        return document
            .Descendants()
            .FirstOrDefault(e => e.Name.LocalName == "book-title")
            ?.Value
            .Trim();
    }

    private static Dictionary<string, ParsedBookImage> ParseImages(XDocument document)
    {
        var images = new Dictionary<string, ParsedBookImage>();

        foreach (var binary in document.Descendants().Where(e => e.Name.LocalName == "binary"))
        {
            var id = binary.Attribute("id")?.Value;
            var contentType = binary.Attribute("content-type")?.Value;
            var base64 = NormalizeBase64(binary.Value);

            if (string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(contentType) ||
                string.IsNullOrWhiteSpace(base64))
            {
                continue;
            }

            images[id] = new ParsedBookImage(id, contentType, base64);
        }

        return images;
    }

    private static void ParseContainer(XElement container, List<ParsedBookBlock> blocks)
    {
        foreach (var element in container.Elements())
        {
            ParseElement(element, blocks);
        }
    }

    private static void ParseElement(XElement element, List<ParsedBookBlock> blocks)
    {
        switch (element.Name.LocalName)
        {
            case "body":
            case "section":
                ParseContainer(element, blocks);
                AddEmptyLine(blocks);
                break;

            case "title":
                ParseTitle(element, blocks);
                break;

            case "subtitle":
                AddTextBlock(blocks, ReadingContentBlockType.Heading2, element.Value);
                break;

            case "p":
                AddTextBlock(blocks, ReadingContentBlockType.Paragraph, element.Value);
                break;

            case "empty-line":
                AddEmptyLine(blocks);
                break;

            case "image":
                AddImageBlock(blocks, element);
                break;

            case "epigraph":
                ParseEpigraph(element, blocks);
                break;

            case "poem":
                ParsePoem(element, blocks);
                break;

            case "stanza":
                ParseStanza(element, blocks);
                break;

            case "v":
                AddTextBlock(blocks, ReadingContentBlockType.Verse, element.Value);
                break;

            case "cite":
                ParseQuote(element, blocks);
                break;

            case "text-author":
                AddTextBlock(blocks, ReadingContentBlockType.Author, element.Value);
                break;

            case "annotation":
                ParseAnnotation(element, blocks);
                break;

            default:
                ParseContainer(element, blocks);
                break;
        }
    }

    private static void ParseTitle(XElement title, List<ParsedBookBlock> blocks)
    {
        foreach (var child in title.Elements())
        {
            if (child.Name.LocalName == "p")
            {
                AddTextBlock(blocks, ReadingContentBlockType.Heading1, child.Value);
            }
            else
            {
                ParseElement(child, blocks);
            }
        }

        AddEmptyLine(blocks);
    }

    private static void ParseEpigraph(XElement epigraph, List<ParsedBookBlock> blocks)
    {
        foreach (var child in epigraph.Elements())
        {
            if (child.Name.LocalName == "p")
            {
                AddTextBlock(blocks, ReadingContentBlockType.Quote, child.Value);
            }
            else if (child.Name.LocalName == "text-author")
            {
                AddTextBlock(blocks, ReadingContentBlockType.Author, child.Value);
            }
            else
            {
                ParseElement(child, blocks);
            }
        }

        AddEmptyLine(blocks);
    }

    private static void ParsePoem(XElement poem, List<ParsedBookBlock> blocks)
    {
        foreach (var child in poem.Elements())
        {
            ParseElement(child, blocks);
        }

        AddEmptyLine(blocks);
    }

    private static void ParseStanza(XElement stanza, List<ParsedBookBlock> blocks)
    {
        foreach (var child in stanza.Elements())
        {
            if (child.Name.LocalName == "v")
            {
                AddTextBlock(blocks, ReadingContentBlockType.Verse, child.Value);
            }
            else
            {
                ParseElement(child, blocks);
            }
        }

        AddEmptyLine(blocks);
    }

    private static void ParseQuote(XElement quote, List<ParsedBookBlock> blocks)
    {
        foreach (var child in quote.Elements())
        {
            if (child.Name.LocalName == "p")
            {
                AddTextBlock(blocks, ReadingContentBlockType.Quote, child.Value);
            }
            else if (child.Name.LocalName == "text-author")
            {
                AddTextBlock(blocks, ReadingContentBlockType.Author, child.Value);
            }
            else
            {
                ParseElement(child, blocks);
            }
        }

        AddEmptyLine(blocks);
    }

    private static void ParseAnnotation(XElement annotation, List<ParsedBookBlock> blocks)
    {
        foreach (var child in annotation.Elements())
        {
            if (child.Name.LocalName == "p")
            {
                AddTextBlock(blocks, ReadingContentBlockType.Quote, child.Value);
            }
            else
            {
                ParseElement(child, blocks);
            }
        }

        AddEmptyLine(blocks);
    }

    private static void AddImageBlock(List<ParsedBookBlock> blocks, XElement image)
    {
        var href =
            image.Attribute(XLink + "href")?.Value ??
            image.Attributes().FirstOrDefault(a => a.Name.LocalName == "href")?.Value;

        if (string.IsNullOrWhiteSpace(href))
            return;

        blocks.Add(new ParsedBookBlock(
            ReadingContentBlockType.Image,
            null,
            ImageId: href.TrimStart('#')));

        AddEmptyLine(blocks);
    }

    private static void AddTextBlock(
        List<ParsedBookBlock> blocks,
        ReadingContentBlockType type,
        string value)
    {
        var text = NormalizeWhitespace(value);

        if (string.IsNullOrWhiteSpace(text))
            return;

        blocks.Add(new ParsedBookBlock(type, text));
    }

    private static void AddEmptyLine(List<ParsedBookBlock> blocks)
    {
        if (blocks.Count == 0)
            return;

        if (blocks[^1].Type == ReadingContentBlockType.EmptyLine)
            return;

        blocks.Add(new ParsedBookBlock(ReadingContentBlockType.EmptyLine, null));
    }

    private static void TrimEmptyLines(List<ParsedBookBlock> blocks)
    {
        while (blocks.Count > 0 && blocks[0].Type == ReadingContentBlockType.EmptyLine)
        {
            blocks.RemoveAt(0);
        }

        while (blocks.Count > 0 && blocks[^1].Type == ReadingContentBlockType.EmptyLine)
        {
            blocks.RemoveAt(blocks.Count - 1);
        }
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

    private static string NormalizeBase64(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            if (!char.IsWhiteSpace(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }
}
