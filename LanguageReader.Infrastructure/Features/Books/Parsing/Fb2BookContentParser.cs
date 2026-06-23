using System.Text;
using System.Xml.Linq;
using LanguageReader.Infrastructure.Features.Books.Parsing.Models;

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
        var addedImageIds = new HashSet<string>(StringComparer.Ordinal);

        var bodies = document
            .Root?
            .Elements()
            .Where(e => e.Name.LocalName == "body")
            .Where(e => e.Attribute("name")?.Value != "notes")
            .ToList() ?? [];

        var blocks = new List<ParsedBookBlock>();
        AddCoverImageBlocks(document, blocks, addedImageIds);

        foreach (var body in bodies)
        {
            var bodyName = body.Attribute("name")?.Value;

            if (!string.IsNullOrWhiteSpace(bodyName))
            {
                AddTextBlock(blocks, ReadingContentBlockType.Heading1, bodyName);
            }

            ParseContainer(body, blocks, addedImageIds);
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
        var images = new Dictionary<string, ParsedBookImage>(StringComparer.OrdinalIgnoreCase);

        foreach (var binary in document.Descendants().Where(e => e.Name.LocalName == "binary"))
        {
            var id = binary.Attribute("id")?.Value?.Trim();
            var contentType = binary.Attribute("content-type")?.Value?.Trim();
            var base64 = NormalizeBase64(binary.Value);

            if (string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(base64))
            {
                continue;
            }

            contentType = NormalizeContentType(contentType, id);
            images[id] = new ParsedBookImage(id, contentType, base64);
        }

        return images;
    }

    private static string NormalizeContentType(string? contentType, string imageId)
    {
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            return string.Equals(contentType, "image/jpg", StringComparison.OrdinalIgnoreCase)
                ? "image/jpeg"
                : contentType;
        }

        var extension = Path.GetExtension(imageId).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            _ => "application/octet-stream"
        };
    }

    private static void AddCoverImageBlocks(
        XDocument document,
        List<ParsedBookBlock> blocks,
        HashSet<string> addedImageIds)
    {
        foreach (var image in document
            .Descendants()
            .Where(element => element.Name.LocalName == "coverpage")
            .Elements()
            .Where(element => element.Name.LocalName == "image"))
        {
            AddImageBlock(blocks, image, addedImageIds);
        }
    }

    private static void ParseContainer(
        XElement container,
        List<ParsedBookBlock> blocks,
        HashSet<string> addedImageIds)
    {
        foreach (var element in container.Elements())
        {
            ParseElement(element, blocks, addedImageIds);
        }
    }

    private static void ParseElement(
        XElement element,
        List<ParsedBookBlock> blocks,
        HashSet<string> addedImageIds)
    {
        switch (element.Name.LocalName)
        {
            case "body":
            case "section":
                ParseContainer(element, blocks, addedImageIds);
                AddEmptyLine(blocks);
                break;

            case "title":
                ParseTitle(element, blocks, addedImageIds);
                break;

            case "subtitle":
                AddTextBlock(blocks, ReadingContentBlockType.Heading2, element.Value);
                break;

            case "p":
                ParseParagraph(element, blocks, addedImageIds);
                break;

            case "empty-line":
                AddEmptyLine(blocks);
                break;

            case "image":
                AddImageBlock(blocks, element, addedImageIds);
                break;

            case "epigraph":
                ParseEpigraph(element, blocks, addedImageIds);
                break;

            case "poem":
                ParsePoem(element, blocks, addedImageIds);
                break;

            case "stanza":
                ParseStanza(element, blocks, addedImageIds);
                break;

            case "v":
                AddTextBlock(blocks, ReadingContentBlockType.Verse, element.Value);
                break;

            case "cite":
                ParseQuote(element, blocks, addedImageIds);
                break;

            case "text-author":
                AddTextBlock(blocks, ReadingContentBlockType.Author, element.Value);
                break;

            case "annotation":
                ParseAnnotation(element, blocks, addedImageIds);
                break;

            default:
                ParseContainer(element, blocks, addedImageIds);
                break;
        }
    }

    private static void ParseTitle(
        XElement title,
        List<ParsedBookBlock> blocks,
        HashSet<string> addedImageIds)
    {
        foreach (var child in title.Elements())
        {
            if (child.Name.LocalName == "p")
            {
                ParseParagraph(child, blocks, addedImageIds, ReadingContentBlockType.Heading1);
            }
            else
            {
                ParseElement(child, blocks, addedImageIds);
            }
        }

        AddEmptyLine(blocks);
    }

    private static void ParseEpigraph(
        XElement epigraph,
        List<ParsedBookBlock> blocks,
        HashSet<string> addedImageIds)
    {
        foreach (var child in epigraph.Elements())
        {
            if (child.Name.LocalName == "p")
            {
                ParseParagraph(child, blocks, addedImageIds, ReadingContentBlockType.Quote);
            }
            else if (child.Name.LocalName == "text-author")
            {
                AddTextBlock(blocks, ReadingContentBlockType.Author, child.Value);
            }
            else
            {
                ParseElement(child, blocks, addedImageIds);
            }
        }

        AddEmptyLine(blocks);
    }

    private static void ParsePoem(
        XElement poem,
        List<ParsedBookBlock> blocks,
        HashSet<string> addedImageIds)
    {
        foreach (var child in poem.Elements())
        {
            ParseElement(child, blocks, addedImageIds);
        }

        AddEmptyLine(blocks);
    }

    private static void ParseStanza(
        XElement stanza,
        List<ParsedBookBlock> blocks,
        HashSet<string> addedImageIds)
    {
        foreach (var child in stanza.Elements())
        {
            if (child.Name.LocalName == "v")
            {
                AddTextBlock(blocks, ReadingContentBlockType.Verse, child.Value);
            }
            else
            {
                ParseElement(child, blocks, addedImageIds);
            }
        }

        AddEmptyLine(blocks);
    }

    private static void ParseQuote(
        XElement quote,
        List<ParsedBookBlock> blocks,
        HashSet<string> addedImageIds)
    {
        foreach (var child in quote.Elements())
        {
            if (child.Name.LocalName == "p")
            {
                ParseParagraph(child, blocks, addedImageIds, ReadingContentBlockType.Quote);
            }
            else if (child.Name.LocalName == "text-author")
            {
                AddTextBlock(blocks, ReadingContentBlockType.Author, child.Value);
            }
            else
            {
                ParseElement(child, blocks, addedImageIds);
            }
        }

        AddEmptyLine(blocks);
    }

    private static void ParseAnnotation(
        XElement annotation,
        List<ParsedBookBlock> blocks,
        HashSet<string> addedImageIds)
    {
        foreach (var child in annotation.Elements())
        {
            if (child.Name.LocalName == "p")
            {
                ParseParagraph(child, blocks, addedImageIds, ReadingContentBlockType.Quote);
            }
            else
            {
                ParseElement(child, blocks, addedImageIds);
            }
        }

        AddEmptyLine(blocks);
    }

    private static void ParseParagraph(
        XElement paragraph,
        List<ParsedBookBlock> blocks,
        HashSet<string> addedImageIds,
        ReadingContentBlockType textBlockType = ReadingContentBlockType.Paragraph)
    {
        var imageChildren = paragraph
            .Elements()
            .Where(element => element.Name.LocalName == "image")
            .ToArray();

        var text = NormalizeWhitespace(string.Concat(
            paragraph
                .Nodes()
                .Where(node => node is not XElement element || element.Name.LocalName != "image")
                .Select(GetNodeText)));

        AddTextBlock(blocks, textBlockType, text);

        foreach (var image in imageChildren)
        {
            AddImageBlock(blocks, image, addedImageIds);
        }
    }

    private static string GetNodeText(XNode node)
    {
        return node switch
        {
            XCData cdata => cdata.Value,
            XText text => text.Value,
            XElement element => element.Value,
            _ => string.Empty
        };
    }

    private static void AddImageBlock(
        List<ParsedBookBlock> blocks,
        XElement image,
        HashSet<string> addedImageIds)
    {
        var href = GetImageHref(image);

        if (string.IsNullOrWhiteSpace(href))
            return;

        var imageId = href.Trim().TrimStart('#');
        if (string.IsNullOrWhiteSpace(imageId) || !addedImageIds.Add(imageId))
            return;

        blocks.Add(new ParsedBookBlock(
            ReadingContentBlockType.Image,
            null,
            ImageId: imageId));

        AddEmptyLine(blocks);
    }

    private static string? GetImageHref(XElement image)
    {
        return image.Attribute(XLink + "href")?.Value
            ?? image.Attributes().FirstOrDefault(attribute =>
                string.Equals(attribute.Name.LocalName, "href", StringComparison.OrdinalIgnoreCase))?.Value;
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
