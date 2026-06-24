using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Common.Language;
using LanguageReader.Infrastructure.Features.News.Services;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Parsing.Models;
using LanguageReader.Infrastructure.Features.ReadingItems.Services;
using Microsoft.EntityFrameworkCore;
using LanguageReader.Api.Features.ReadingItems;

namespace LanguageReader.Api.Features.News;

internal sealed class ImportNewsArticleHandler(
    ApplicationDbContext dbContext,
    IArticleImportService articleImportService,
    ReadingItemDocumentStorageService documentStorage)
{
    public async Task<ReadingItemDetailsDto> HandleAsync(ImportNewsArticleRequest request, CancellationToken ct)
    {
        var username = UsernameHelper.Require(request.Username);
        var normalizedSourceKey = request.SourceKey.Trim().ToLowerInvariant();
        var normalizedRequestUrl = request.Url.Trim();
        var now = DateTimeOffset.UtcNow;
        var extracted = await articleImportService.ExtractAsync(request.SourceKey, request.Url, ct);
        var normalizedLanguage = LanguageNameNormalizer.Normalize(extracted.OriginalLanguage);
        var candidate = await dbContext.RssArticleCandidates
            .FirstOrDefaultAsync(item =>
                item.SourceKey == normalizedSourceKey
                && item.Url == normalizedRequestUrl,
                ct);

        var existing = await dbContext.ReadingItems
            .Include(item => item.ArticleMetadata)
            .FirstOrDefaultAsync(item =>
                item.OwnerUsername == username
                && item.ArticleMetadata != null
                && item.ArticleMetadata.OriginalUrl == extracted.OriginalUrl,
                ct);

        if (existing is not null)
        {
            if (candidate is not null)
            {
                candidate.Status = NewsArticleStatus.Saved;
                candidate.SavedReadingItemId = existing.Id;
                candidate.UpdatedAtUtc = now;
                await dbContext.SaveChangesAsync(ct);
            }

            return new ReadingItemDetailsDto(
                existing.Id,
                existing.Title,
                existing.Type,
                LanguageNameNormalizer.Normalize(existing.OriginalLanguage),
                existing.IsPublic,
                existing.CreatedAtUtc,
                existing.UpdatedAtUtc,
                ReadingItemFeatureHelpers.ResolveSourceKey(
                    existing.ArticleMetadata?.SourceName,
                    existing.ArticleMetadata?.RssFeedUrl,
                    existing.ArticleMetadata?.OriginalUrl),
                existing.ArticleMetadata?.SourceName,
                existing.ArticleMetadata?.Author,
                existing.ArticleMetadata?.PublishedAtUtc,
                existing.ArticleMetadata?.OriginalUrl,
                existing.ArticleMetadata?.ImageUrl,
                existing.ArticleMetadata?.Excerpt,
                existing.ArticleMetadata?.RssFeedUrl,
                existing.ArticleMetadata?.ExternalId);
        }

        var readingItemId = Guid.NewGuid();

        var readingItem = new ReadingItemEntity
        {
            Id = readingItemId,
            OwnerUsername = username,
            Title = extracted.Title,
            OriginalLanguage = normalizedLanguage,
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
        dbContext.ArticleMetadata.Add(metadata);

        if (candidate is not null)
        {
            candidate.Status = NewsArticleStatus.Saved;
            candidate.SavedReadingItemId = readingItemId;
            candidate.UpdatedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(ct);

        return new ReadingItemDetailsDto(
            readingItem.Id,
            readingItem.Title,
            readingItem.Type,
            normalizedLanguage,
            readingItem.IsPublic,
            readingItem.CreatedAtUtc,
            readingItem.UpdatedAtUtc,
            extracted.SourceKey,
            metadata.SourceName,
            metadata.Author,
            metadata.PublishedAtUtc,
            metadata.OriginalUrl,
            metadata.ImageUrl,
            metadata.Excerpt,
            metadata.RssFeedUrl,
            metadata.ExternalId);
    }
}
