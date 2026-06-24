using LanguageReader.Infrastructure.Features.Common.Language;
using LanguageReader.Infrastructure.Features.News.Models;
using LanguageReader.Infrastructure.Features.ReadingItems.Parsing.Models;

namespace LanguageReader.Api.Features.ReadingItems;

internal static class ReadingItemImportPreviewMappingExtensions
{
    public static ReadingItemImportPreviewDto ToImportPreviewDto(
        this ParsedReadingDocument document,
        string fallbackTitle,
        string originalLanguage,
        string? originalUrl = null)
    {
        var textBlocks = document.Blocks
            .Where(block => !string.IsNullOrWhiteSpace(block.Text))
            .ToList();
        var excerpt = BuildExcerpt(textBlocks.Select(block => block.Text));
        var title = string.IsNullOrWhiteSpace(document.Title)
            ? fallbackTitle
            : document.Title.Trim();

        return new ReadingItemImportPreviewDto(
            title,
            ReadingItemType.Book,
            LanguageNameNormalizer.Normalize(originalLanguage),
            SourceName: null,
            originalUrl,
            Author: null,
            PublishedAtUtc: null,
            ImageUrl: null,
            excerpt,
            textBlocks.Count,
            document.Assets.Count);
    }

    public static ReadingItemImportPreviewDto ToImportPreviewDto(this ExtractedArticleContent article)
    {
        return new ReadingItemImportPreviewDto(
            article.Title,
            ReadingItemType.Article,
            LanguageNameNormalizer.Normalize(article.OriginalLanguage),
            article.SourceName,
            article.OriginalUrl,
            article.Author,
            article.PublishedAtUtc,
            article.ImageUrl,
            string.IsNullOrWhiteSpace(article.Excerpt)
                ? BuildExcerpt(article.Paragraphs)
                : article.Excerpt,
            article.Paragraphs.Count,
            string.IsNullOrWhiteSpace(article.ImageUrl) ? 0 : 1);
    }

    private static string? BuildExcerpt(IEnumerable<string?> blocks)
    {
        var text = string.Join(
            " ",
            blocks
                .Where(block => !string.IsNullOrWhiteSpace(block))
                .Select(block => block!.Trim()));

        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return text.Length <= 260 ? text : text[..260].TrimEnd() + "...";
    }
}
