using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.News.Models;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Parsing.Models;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public sealed class ReadingItemImportService(
    ApplicationDbContext dbContext,
    ReadingItemDocumentStorageService documentStorage)
{
    public async Task<ReadingItemEntity> SaveBookAsync(
        string username,
        string? requestedTitle,
        string? requestedOriginalLanguage,
        string fallbackFileName,
        ParsedReadingDocument parsedDocument,
        CancellationToken cancellationToken)
    {
        var readingItemId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var title = string.IsNullOrWhiteSpace(requestedTitle)
            ? parsedDocument.Title ?? Path.GetFileNameWithoutExtension(fallbackFileName)
            : requestedTitle.Trim();

        var readingItem = new ReadingItemEntity
        {
            Id = readingItemId,
            OwnerUsername = username,
            Title = title,
            OriginalLanguage = NormalizeLanguage(requestedOriginalLanguage),
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
            cancellationToken);

        dbContext.ReadingItems.Add(readingItem);
        await dbContext.SaveChangesAsync(cancellationToken);

        return readingItem;
    }

    public async Task<ReadingItemEntity> SaveArticleAsync(
        string username,
        ExtractedArticleContent extracted,
        string? requestedTitle,
        string? requestedOriginalLanguage,
        CancellationToken cancellationToken)
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

        await documentStorage.StoreAsync(
            readingItem,
            BuildSourceBlocks(extracted.Paragraphs),
            new Dictionary<string, ParsedReadingAsset>(),
            coverImageId: null,
            now,
            cancellationToken);

        dbContext.ReadingItems.Add(readingItem);
        await dbContext.SaveChangesAsync(cancellationToken);

        return readingItem;
    }

    private static IReadOnlyList<ReadingContentBlockDto> BuildSourceBlocks(IReadOnlyList<string> paragraphs)
    {
        return paragraphs
            .Where(paragraph => !string.IsNullOrWhiteSpace(paragraph))
            .Select(paragraph => new ReadingContentBlockDto(
                ReadingContentBlockType.Paragraph,
                paragraph.Trim(),
                ImageId: null))
            .ToArray();
    }

    private static string NormalizeLanguage(string? language)
    {
        return SupportedLanguages.Normalize(language);
    }
}
