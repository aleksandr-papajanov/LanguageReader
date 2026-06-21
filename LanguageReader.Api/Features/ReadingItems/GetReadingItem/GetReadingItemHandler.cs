using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Common.Language;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class GetReadingItemHandler(ApplicationDbContext dbContext)
{
    public async Task<ReadingItemDetailsDto> HandleAsync(GetReadingItemRequest request, CancellationToken ct)
    {
        var item = await dbContext.ReadingItems
            .AsNoTracking()
            .Include(candidate => candidate.ArticleMetadata)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.ReadingItemId, ct);

        if (item is null)
        {
            throw new NotFoundException($"Reading item '{request.ReadingItemId}' was not found.");
        }

        if (!ReadingItemFeatureHelpers.CanRead(item, request.Username))
        {
            throw new ForbiddenException("You do not have access to this reading item.");
        }

        return new ReadingItemDetailsDto(
            item.Id,
            item.Title,
            item.Type,
            LanguageNameNormalizer.Normalize(item.OriginalLanguage),
            item.IsPublic,
            item.CreatedAtUtc,
            item.UpdatedAtUtc,
            ReadingItemFeatureHelpers.ResolveSourceKey(
                item.ArticleMetadata?.SourceName,
                item.ArticleMetadata?.RssFeedUrl,
                item.ArticleMetadata?.OriginalUrl),
            item.ArticleMetadata?.SourceName,
            item.ArticleMetadata?.Author,
            item.ArticleMetadata?.PublishedAtUtc,
            item.ArticleMetadata?.OriginalUrl,
            item.ArticleMetadata?.ImageUrl,
            item.ArticleMetadata?.Excerpt,
            item.ArticleMetadata?.RssFeedUrl,
            item.ArticleMetadata?.ExternalId);
    }
}
