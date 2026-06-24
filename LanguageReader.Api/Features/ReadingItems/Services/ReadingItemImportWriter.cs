using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.News.Models;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Parsing.Models;
using LanguageReader.Infrastructure.Features.ReadingItems.Services;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class ReadingItemImportWriter(
    ApplicationDbContext dbContext,
    ReadingItemDocumentStorageService documentStorage,
    ReadingItemApiUrlBuilder apiUrls)
{
    public async Task<ReadingItemDetailsDto> SaveBookAsync(
        string username,
        string? requestedTitle,
        string? requestedOriginalLanguage,
        string fallbackFileName,
        ParsedReadingDocument parsedDocument,
        CancellationToken ct)
    {
        var readingItemId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var title = string.IsNullOrWhiteSpace(requestedTitle)
            ? parsedDocument.Title ?? Path.GetFileNameWithoutExtension(fallbackFileName)
            : requestedTitle.Trim();
        var originalLanguage = NormalizeLanguage(requestedOriginalLanguage);

        var readingItem = new ReadingItemEntity
        {
            Id = readingItemId,
            OwnerUsername = username,
            Title = title,
            OriginalLanguage = originalLanguage,
            StoragePath = string.Empty,
            Type = ReadingItemType.Book,
            ContentFormat = ReadingContentFormat.Canonical,
            IsPublic = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var sourceBlocks = parsedDocument.Blocks
            .Select(block => new ReadingContentBlockDto(block.Type, block.Text, block.ImageId))
            .ToArray();
        var coverImageId = sourceBlocks
            .FirstOrDefault(block => block.Type == ReadingContentBlockType.Image && !string.IsNullOrWhiteSpace(block.ImageId))
            ?.ImageId;

        await documentStorage.StoreAsync(
            readingItem,
            sourceBlocks,
            parsedDocument.Assets,
            parsedDocument.CoverAssetId ?? coverImageId,
            now,
            ct);

        dbContext.ReadingItems.Add(readingItem);
        await dbContext.SaveChangesAsync(ct);

        return readingItem.ToReadingItemDetailsDto(username, apiUrls);
    }

    public async Task<ReadingItemDetailsDto> SaveArticleAsync(
        string username,
        ExtractedArticleContent extracted,
        string? requestedTitle,
        string? requestedOriginalLanguage,
        CancellationToken ct)
    {
        var readingItemId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var originalLanguage = NormalizeLanguage(
            string.IsNullOrWhiteSpace(requestedOriginalLanguage)
                ? extracted.OriginalLanguage
                : requestedOriginalLanguage);

        var readingItem = new ReadingItemEntity
        {
            Id = readingItemId,
            OwnerUsername = username,
            Title = string.IsNullOrWhiteSpace(requestedTitle)
                ? extracted.Title
                : requestedTitle.Trim(),
            OriginalLanguage = originalLanguage,
            StoragePath = string.Empty,
            Type = ReadingItemType.Article,
            ContentFormat = ReadingContentFormat.Canonical,
            IsPublic = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var metadata = new ArticleMetadataEntity
        {
            ReadingItemId = readingItemId,
            SourceName = extracted.SourceName,
            OriginalUrl = extracted.OriginalUrl,
            PublishedAtUtc = extracted.PublishedAtUtc?.ToUniversalTime(),
            Author = extracted.Author,
            ImageUrl = extracted.ImageUrl,
            Excerpt = extracted.Excerpt,
            RssFeedUrl = extracted.RssFeedUrl,
            ExternalId = extracted.ExternalId
        };
        readingItem.ArticleMetadata = metadata;

        var sourceBlocks = extracted.Paragraphs
            .Where(paragraph => !string.IsNullOrWhiteSpace(paragraph))
            .Select(paragraph => new ReadingContentBlockDto(
                ReadingContentBlockType.Paragraph,
                paragraph.Trim(),
                ImageId: null))
            .ToArray();

        await documentStorage.StoreAsync(
            readingItem,
            sourceBlocks,
            new Dictionary<string, ParsedReadingAsset>(),
            coverImageId: null,
            now,
            ct);

        dbContext.ReadingItems.Add(readingItem);
        await dbContext.SaveChangesAsync(ct);

        return readingItem.ToReadingItemDetailsDto(username, apiUrls);
    }

    private static string NormalizeLanguage(string? language)
    {
        return SupportedLanguages.Normalize(language);
    }
}
