using System.Globalization;
using System.Xml.Linq;
using HtmlAgilityPack;
using LanguageReader.Infrastructure.Features.Common.Language;
using LanguageReader.Infrastructure.Features.News.Models;
using SmartReader;

namespace LanguageReader.Infrastructure.Features.News.Services;

public sealed class ArticleImportService(HttpClient httpClient) : IArticleImportService
{
    private const string ReaderUserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36";

    private static readonly IReadOnlyDictionary<string, NewsFeedSourceDefinition> Sources =
        new Dictionary<string, NewsFeedSourceDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            [NewsSourceKeys.Svt] = new(
                NewsSourceKeys.Svt,
                "SVT",
                "https://www.svt.se/nyheter/rss.xml",
                "Swedish"),
            [NewsSourceKeys.Aftonbladet] = new(
                NewsSourceKeys.Aftonbladet,
                "Aftonbladet",
                "https://rss.aftonbladet.se/rss2/small/pages/sections/senastenytt/",
                "Swedish"),
            [NewsSourceKeys.SverigesRadio] = new(
                NewsSourceKeys.SverigesRadio,
                "Sveriges Radio",
                "https://api.sr.se/api/rss/program/83",
                "Swedish")
        };

    public async Task<ExtractedArticleContent> ExtractAsync(string sourceKey, string url, CancellationToken cancellationToken = default)
    {
        var source = ResolveSource(sourceKey);
        string sourceHtml;

        try
        {
            sourceHtml = await httpClient.GetStringAsync(url, cancellationToken);
        }
        catch (HttpRequestException) when (string.Equals(source.SourceKey, NewsSourceKeys.SverigesRadio, StringComparison.OrdinalIgnoreCase))
        {
            return await ExtractSverigesRadioFromFeedAsync(source, url, cancellationToken);
        }

        var sourceDocument = ParseHtmlDocument(sourceHtml);

        var article = await Task.Run(() => Reader.ParseArticle(url, sourceHtml, ReaderUserAgent), cancellationToken);
        if (!article.Completed && article.Errors.Count > 0)
        {
            throw new InvalidOperationException("Unable to download or parse article content.", article.Errors[0]);
        }

        if (!article.IsReadable)
        {
            throw new InvalidOperationException("Unable to extract article text.");
        }

        var title = NormalizeText(article.Title)
            ?? FirstContent(sourceDocument,
                "//meta[@property='og:title']/@content",
                "//meta[@name='twitter:title']/@content",
                "//h1",
                "//title")
            ?? throw new InvalidOperationException("Unable to extract article title.");

        var paragraphs = ExtractParagraphs(article.Content, article.TextContent);
        if (paragraphs.Count == 0)
        {
            throw new InvalidOperationException("Unable to extract article text.");
        }

        var excerpt = NormalizeText(article.Excerpt)
            ?? FirstContent(sourceDocument,
                "//meta[@property='og:description']/@content",
                "//meta[@name='description']/@content");

        return new ExtractedArticleContent(
            source.SourceKey,
            source.SourceName,
            url,
            title,
            LanguageNameNormalizer.Normalize(source.DefaultLanguage),
            paragraphs,
            NormalizeText(article.Author)
                ?? FirstContent(sourceDocument,
                    "//meta[@name='author']/@content",
                    "//meta[@property='article:author']/@content"),
            NormalizeText(article.FeaturedImage)
                ?? FirstContent(sourceDocument,
                    "//meta[@property='og:image']/@content",
                    "//meta[@name='twitter:image']/@content"),
            excerpt,
            ParseDate(FirstContent(sourceDocument,
                "//meta[@property='article:published_time']/@content",
                "//time/@datetime"))
                ?? NormalizePublicationDate(article.PublicationDate),
            FirstContent(sourceDocument,
                "//meta[@property='article:id']/@content",
                "//meta[@name='article:id']/@content"),
            source.RssFeedUrl);
    }

    public async Task<NewsArticlePreviewMetadata?> TryExtractPreviewAsync(
        string sourceKey,
        string url,
        CancellationToken cancellationToken = default)
    {
        _ = ResolveSource(sourceKey);

        try
        {
            var sourceDocument = await TryLoadSourceDocumentAsync(url, cancellationToken);
            if (sourceDocument is null)
            {
                return null;
            }

            var author = FirstContent(sourceDocument,
                "//meta[@name='author']/@content",
                "//meta[@property='article:author']/@content");
            var imageUrl = NormalizeUrl(url, FirstContent(sourceDocument,
                "//meta[@property='og:image']/@content",
                "//meta[@name='twitter:image']/@content"));
            var publishedAtUtc = ParseDate(FirstContent(sourceDocument,
                "//meta[@property='article:published_time']/@content",
                "//time/@datetime"));

            if (string.IsNullOrWhiteSpace(author)
                && string.IsNullOrWhiteSpace(imageUrl)
                && !publishedAtUtc.HasValue)
            {
                return null;
            }

            return new NewsArticlePreviewMetadata(author, imageUrl, publishedAtUtc);
        }
        catch
        {
            return null;
        }
    }

    private static NewsFeedSourceDefinition ResolveSource(string sourceKey)
    {
        if (Sources.TryGetValue(sourceKey?.Trim() ?? string.Empty, out var source))
        {
            return source;
        }

        throw new InvalidOperationException($"Unsupported news source '{sourceKey}'.");
    }

    private async Task<HtmlDocument?> TryLoadSourceDocumentAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            var html = await httpClient.GetStringAsync(url, cancellationToken);
            return ParseHtmlDocument(html);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private static HtmlDocument ParseHtmlDocument(string html)
    {
        var sourceDocument = new HtmlDocument();
        sourceDocument.LoadHtml(html);
        return sourceDocument;
    }

    private async Task<ExtractedArticleContent> ExtractSverigesRadioFromFeedAsync(
        NewsFeedSourceDefinition source,
        string url,
        CancellationToken cancellationToken)
    {
        XNamespace atomNs = "http://www.w3.org/2005/Atom";
        var feedXml = await httpClient.GetStringAsync(source.RssFeedUrl, cancellationToken);
        var document = XDocument.Parse(feedXml);
        var normalizedUrl = url.Trim();
        var articleId = normalizedUrl.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

        var entry = document.Root?
            .Elements(atomNs + "entry")
            .FirstOrDefault(candidate =>
            {
                var entryUrl = candidate.Elements(atomNs + "link")
                    .Select(element => element.Attribute("href")?.Value?.Trim())
                    .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

                if (string.Equals(entryUrl, normalizedUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                var entryId = candidate.Element(atomNs + "id")?.Value;
                return !string.IsNullOrWhiteSpace(articleId)
                    && !string.IsNullOrWhiteSpace(entryId)
                    && entryId.Contains(articleId, StringComparison.OrdinalIgnoreCase);
            });

        if (entry is null)
        {
            throw new InvalidOperationException("Unable to extract article text.");
        }

        var title = NormalizeText(entry.Element(atomNs + "title")?.Value)
            ?? throw new InvalidOperationException("Unable to extract article title.");
        var contentHtml = entry.Element(atomNs + "content")?.Value;
        var paragraphs = ExtractParagraphs(contentHtml, null)
            .Where(paragraph =>
                !paragraph.StartsWith("Lyssna:", StringComparison.OrdinalIgnoreCase)
                && !paragraph.Contains("@sverigesradio.se", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (paragraphs.Count == 0)
        {
            throw new InvalidOperationException("Unable to extract article text.");
        }

        return new ExtractedArticleContent(
            source.SourceKey,
            source.SourceName,
            normalizedUrl,
            title,
            source.DefaultLanguage,
            paragraphs,
            NormalizeText(entry.Element(atomNs + "author")?.Element(atomNs + "name")?.Value),
            ExtractFirstImageUrlFromHtml(contentHtml),
            HtmlToPlainText(entry.Element(atomNs + "summary")?.Value),
            ParseDate(entry.Element(atomNs + "published")?.Value ?? entry.Element(atomNs + "updated")?.Value),
            NormalizeText(entry.Element(atomNs + "id")?.Value),
            source.RssFeedUrl);
    }

    private static List<string> ExtractParagraphs(string? extractedHtml, string? textContent)
    {
        if (!string.IsNullOrWhiteSpace(extractedHtml))
        {
            var extractedDocument = new HtmlDocument();
            extractedDocument.LoadHtml(extractedHtml);

            var paragraphNodes = extractedDocument.DocumentNode.SelectNodes("//p|//li|//blockquote");
            var extractedParagraphs = (paragraphNodes?.ToList() ?? [])
                .Select(node => NormalizeText(node.InnerText))
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Cast<string>()
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (extractedParagraphs.Count > 0)
            {
                return extractedParagraphs;
            }
        }

        return SplitTextContent(textContent);
    }

    private static List<string> SplitTextContent(string? textContent)
    {
        if (string.IsNullOrWhiteSpace(textContent))
        {
            return [];
        }

        var paragraphs = textContent
            .Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeText)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (paragraphs.Count > 1)
        {
            return paragraphs;
        }

        return textContent
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizeText)
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static string? FirstContent(HtmlDocument? document, params string[] xpaths)
    {
        if (document?.DocumentNode is null)
        {
            return null;
        }

        foreach (var xpath in xpaths)
        {
            var node = document.DocumentNode.SelectSingleNode(xpath);
            if (node is null)
            {
                continue;
            }

            var value = NormalizeText(
                node.GetAttributeValue("content", string.Empty)
                ?? node.GetAttributeValue("lang", string.Empty)
                ?? node.InnerText);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string? NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = HtmlEntity.DeEntitize(value).Trim();
        normalized = string.Join(" ", normalized.Split(['\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string? HtmlToPlainText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var document = new HtmlDocument();
        document.LoadHtml(value);
        return NormalizeText(document.DocumentNode.InnerText);
    }

    private static string? ExtractFirstImageUrlFromHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var document = ParseHtmlDocument(html);
        var imageNode = document.DocumentNode.SelectSingleNode("//img[@src]");
        var imageUrl = imageNode?.GetAttributeValue("src", string.Empty);
        return NormalizeUrl("https://www.sverigesradio.se", imageUrl);
    }

    private static DateTimeOffset? ParseDate(string? value)
    {
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var parsed)
            ? parsed.ToUniversalTime()
            : null;
    }

    private static DateTimeOffset? NormalizePublicationDate(DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        var publicationDate = value.Value;
        var utcDate = publicationDate.Kind switch
        {
            DateTimeKind.Utc => publicationDate,
            DateTimeKind.Local => publicationDate.ToUniversalTime(),
            _ => DateTime.SpecifyKind(publicationDate, DateTimeKind.Utc)
        };

        return new DateTimeOffset(utcDate);
    }

    private static string? NormalizeUrl(string pageUrl, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalizedValue = value.Trim();

        if (Uri.TryCreate(normalizedValue, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        return Uri.TryCreate(new Uri(pageUrl), normalizedValue, out var relativeUri)
            ? relativeUri.ToString()
            : normalizedValue;
    }
}
