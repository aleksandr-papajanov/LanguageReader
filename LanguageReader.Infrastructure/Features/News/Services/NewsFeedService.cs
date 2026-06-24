using System.Globalization;
using System.Xml.Linq;
using HtmlAgilityPack;
using LanguageReader.Infrastructure.Features.News.Models;

namespace LanguageReader.Infrastructure.Features.News.Services;

public sealed class NewsFeedService(HttpClient httpClient) : INewsFeedService
{
    private static readonly Uri SvtBaseUri = new("https://www.svt.se");
    private static readonly Uri AftonbladetBaseUri = new("https://www.aftonbladet.se");
    private static readonly Uri SverigesRadioBaseUri = new("https://www.sverigesradio.se");
    private static readonly Uri OttasidorBaseUri = new("https://8sidor.se");

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
                "Swedish"),
            [NewsSourceKeys.Ottasidor] = new(
                NewsSourceKeys.Ottasidor,
                "8 sidor",
                "https://8sidor.se/feed/",
                "Swedish")
        };

    public async Task<IReadOnlyList<FetchedNewsArticle>> FetchAsync(string sourceKey, CancellationToken cancellationToken = default)
    {
        var source = ResolveSource(sourceKey);
        var xml = await httpClient.GetStringAsync(source.RssFeedUrl, cancellationToken);
        var document = XDocument.Parse(xml);

        return source.SourceKey switch
        {
            NewsSourceKeys.Svt => ParseRssFeed(document, source, preferEnclosure: false),
            NewsSourceKeys.Aftonbladet => ParseRssFeed(document, source, preferEnclosure: true),
            NewsSourceKeys.SverigesRadio => ParseSverigesRadioFeed(document, source),
            NewsSourceKeys.Ottasidor => ParseRssFeed(document, source, preferEnclosure: true),
            _ => throw new InvalidOperationException($"Unsupported news source '{sourceKey}'.")
        };
    }

    private static IReadOnlyList<FetchedNewsArticle> ParseRssFeed(
        XDocument document,
        NewsFeedSourceDefinition source,
        bool preferEnclosure)
    {
        XNamespace mediaNs = "http://search.yahoo.com/mrss/";

        return document.Root?
            .Descendants("item")
            .Select(item => new FetchedNewsArticle(
                source.SourceKey,
                source.SourceName,
                (item.Element("title")?.Value ?? string.Empty).Trim(),
                (item.Element("link")?.Value ?? string.Empty).Trim(),
                NormalizeText(item.Element("description")?.Value),
                NormalizeText(item.Element("guid")?.Value),
                ParseDate(item.Element("pubDate")?.Value),
                NormalizeText(item.Element("author")?.Value),
                ResolveRssImageUrl(item, source.SourceKey, mediaNs, preferEnclosure),
                source.RssFeedUrl))
            .Where(item => !string.IsNullOrWhiteSpace(item.Title) && !string.IsNullOrWhiteSpace(item.Url))
            .ToList()
            ?? [];
    }

    private static IReadOnlyList<FetchedNewsArticle> ParseSverigesRadioFeed(
        XDocument document,
        NewsFeedSourceDefinition source)
    {
        XNamespace atomNs = "http://www.w3.org/2005/Atom";

        return document.Root?
            .Elements(atomNs + "entry")
            .Select(entry => new FetchedNewsArticle(
                source.SourceKey,
                source.SourceName,
                NormalizeText(entry.Element(atomNs + "title")?.Value) ?? string.Empty,
                ResolveAtomLink(entry, atomNs),
                HtmlToText(entry.Element(atomNs + "summary")?.Value),
                NormalizeText(entry.Element(atomNs + "id")?.Value),
                ParseDate(entry.Element(atomNs + "published")?.Value ?? entry.Element(atomNs + "updated")?.Value),
                NormalizeText(entry.Element(atomNs + "author")?.Element(atomNs + "name")?.Value),
                ExtractSverigesRadioImageUrl(entry, atomNs),
                source.RssFeedUrl))
            .Where(item => !string.IsNullOrWhiteSpace(item.Title) && !string.IsNullOrWhiteSpace(item.Url))
            .ToList()
            ?? [];
    }

    private static NewsFeedSourceDefinition ResolveSource(string sourceKey)
    {
        if (Sources.TryGetValue(sourceKey?.Trim() ?? string.Empty, out var source))
        {
            return source;
        }

        throw new InvalidOperationException($"Unsupported news source '{sourceKey}'.");
    }

    private static string ResolveAtomLink(XElement entry, XNamespace atomNs)
    {
        return entry.Elements(atomNs + "link")
            .Select(element => element.Attribute("href")?.Value)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))
            ?.Trim()
            ?? string.Empty;
    }

    private static string? ResolveRssImageUrl(
        XElement item,
        string sourceKey,
        XNamespace mediaNs,
        bool preferEnclosure)
    {
        var enclosureUrl = item.Element("enclosure")?.Attribute("url")?.Value;
        var mediaContentUrl = item.Element(mediaNs + "content")?.Attribute("url")?.Value;
        var mediaThumbnailUrl = item.Element(mediaNs + "thumbnail")?.Attribute("url")?.Value;
        var descriptionUrl = ExtractImageUrl(item.Element("description")?.Value, sourceKey);

        var imageUrl = preferEnclosure
            ? enclosureUrl ?? mediaContentUrl ?? mediaThumbnailUrl ?? descriptionUrl
            : mediaContentUrl ?? mediaThumbnailUrl ?? enclosureUrl ?? descriptionUrl;

        return NormalizeImageUrl(sourceKey, imageUrl);
    }

    private static string? ExtractSverigesRadioImageUrl(XElement entry, XNamespace atomNs)
    {
        return ExtractImageUrl(entry.Element(atomNs + "content")?.Value, NewsSourceKeys.SverigesRadio);
    }

    private static DateTimeOffset? ParseDate(string? value)
    {
        return DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var parsed)
            ? parsed.ToUniversalTime()
            : null;
    }

    private static string? NormalizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return System.Net.WebUtility.HtmlDecode(value).Trim();
    }

    private static string? HtmlToText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var document = new HtmlDocument();
        document.LoadHtml(value);

        var text = NormalizeText(document.DocumentNode.InnerText);
        return string.IsNullOrWhiteSpace(text) ? NormalizeText(value) : text;
    }

    private static string? ExtractImageUrl(string? descriptionHtml, string sourceKey)
    {
        if (string.IsNullOrWhiteSpace(descriptionHtml))
        {
            return null;
        }

        var document = new HtmlDocument();
        document.LoadHtml(descriptionHtml);
        var imageNode = document.DocumentNode.SelectSingleNode("//img[@src]") ?? document.DocumentNode.SelectSingleNode("//img[@data-src]");
        var imageUrl = imageNode?.GetAttributeValue("src", string.Empty)
            ?? imageNode?.GetAttributeValue("data-src", string.Empty)
            ?? ExtractFirstSrcFromSrcSet(imageNode?.GetAttributeValue("srcset", string.Empty));

        return NormalizeImageUrl(sourceKey, imageUrl);
    }

    private static string? ExtractFirstSrcFromSrcSet(string? srcSet)
    {
        if (string.IsNullOrWhiteSpace(srcSet))
        {
            return null;
        }

        return srcSet.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(entry => entry.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault())
            .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url));
    }

    private static string? NormalizeImageUrl(string sourceKey, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var imageUrl = value.Trim();

        if (imageUrl.StartsWith("//", StringComparison.Ordinal))
        {
            return $"https:{imageUrl}";
        }

        if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri.ToString();
        }

        var baseUri = sourceKey switch
        {
            NewsSourceKeys.Svt => SvtBaseUri,
            NewsSourceKeys.Aftonbladet => AftonbladetBaseUri,
            NewsSourceKeys.SverigesRadio => SverigesRadioBaseUri,
            NewsSourceKeys.Ottasidor => OttasidorBaseUri,
            _ => null
        };

        if (baseUri is not null && Uri.TryCreate(baseUri, imageUrl, out var relativeUri))
        {
            return relativeUri.ToString();
        }

        return imageUrl;
    }
}
