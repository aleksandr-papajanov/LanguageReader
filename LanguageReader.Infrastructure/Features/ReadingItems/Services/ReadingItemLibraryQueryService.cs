using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Reading.Services;
using LanguageReader.Infrastructure.Features.ReadingItems.Models;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public sealed class ReadingItemLibraryQueryService(
    ApplicationDbContext dbContext,
    ReadingProgressService readingProgress)
{
    public async Task<IReadOnlyList<ReadingItemLibraryQueryResult>> LoadAsync(
        GetReadingItemsRequest request,
        string normalizedUsername,
        CancellationToken cancellationToken)
    {
        if (request.Ownership == ReadingItemOwnershipFilter.Mine && string.IsNullOrWhiteSpace(normalizedUsername))
        {
            return [];
        }

        var query = dbContext.ReadingItems
            .AsNoTracking()
            .Include(item => item.ArticleMetadata)
            .Include(item => item.Assets)
            .AsQueryable();

        query = request.Ownership switch
        {
            ReadingItemOwnershipFilter.Mine => query.Where(item => item.OwnerUsername == normalizedUsername),
            ReadingItemOwnershipFilter.Public => query.Where(item => item.IsPublic),
            _ => string.IsNullOrWhiteSpace(normalizedUsername)
                ? query.Where(item => item.IsPublic)
                : query.Where(item => item.IsPublic || item.OwnerUsername == normalizedUsername)
        };

        if (request.Type.HasValue)
        {
            query = query.Where(item => item.Type == request.Type.Value);
        }

        query = request.Visibility switch
        {
            ReadingItemVisibilityFilter.Public => query.Where(item => item.IsPublic),
            ReadingItemVisibilityFilter.Private => query.Where(item => !item.IsPublic),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var queryText = request.Query.Trim();
            query = query.Where(item =>
                EF.Functions.ILike(item.Title, $"%{queryText}%")
                || (item.ArticleMetadata != null && (
                    EF.Functions.ILike(item.ArticleMetadata.SourceName ?? string.Empty, $"%{queryText}%")
                    || EF.Functions.ILike(item.ArticleMetadata.Author ?? string.Empty, $"%{queryText}%")
                    || EF.Functions.ILike(item.ArticleMetadata.Excerpt ?? string.Empty, $"%{queryText}%"))));
        }

        var readingItems = await query
            .OrderByDescending(item => item.UpdatedAtUtc)
            .ThenByDescending(item => item.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var progressByItemId = await readingProgress.LoadByItemIdAsync(
            readingItems.Select(item => item.Id),
            normalizedUsername,
            cancellationToken);

        return readingItems
            .Select(item => new ReadingItemLibraryQueryResult(
                item,
                progressByItemId.GetValueOrDefault(item.Id)))
            .ToList();
    }
}
