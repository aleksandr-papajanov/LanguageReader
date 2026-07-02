using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public sealed class ReadingItemAccessService(ApplicationDbContext dbContext)
{
    public async Task<ReadingItemEntity> LoadReadableReadOnlyAsync(
        Guid readingItemId,
        string? username,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.ReadingItems
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == readingItemId, cancellationToken);

        if (item is null)
        {
            throw new NotFoundException($"Reading item '{readingItemId}' was not found.");
        }

        if (!ReadingItemAccessPolicy.CanRead(item, username))
        {
            throw new ForbiddenException("You do not have access to this reading item.");
        }

        return item;
    }

    public async Task<ReadingItemEntity> LoadReadableAsync(
        Guid readingItemId,
        string? username,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.ReadingItems
            .FirstOrDefaultAsync(candidate => candidate.Id == readingItemId, cancellationToken);

        if (item is null)
        {
            throw new NotFoundException($"Reading item '{readingItemId}' was not found.");
        }

        if (!ReadingItemAccessPolicy.CanRead(item, username))
        {
            throw new ForbiddenException("You do not have access to this reading item.");
        }

        return item;
    }

    public async Task<ReadingItemEntity> LoadDetailsReadableReadOnlyAsync(
        Guid readingItemId,
        string? username,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.ReadingItems
            .AsNoTracking()
            .Include(candidate => candidate.ArticleMetadata)
            .Include(candidate => candidate.Assets)
            .FirstOrDefaultAsync(candidate => candidate.Id == readingItemId, cancellationToken);

        if (item is null)
        {
            throw new NotFoundException($"Reading item '{readingItemId}' was not found.");
        }

        if (!ReadingItemAccessPolicy.CanRead(item, username))
        {
            throw new ForbiddenException("You do not have access to this reading item.");
        }

        return item;
    }

    public async Task<ReadingItemEntity> LoadOwnedAsync(
        Guid readingItemId,
        string username,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.ReadingItems
            .FirstOrDefaultAsync(candidate => candidate.Id == readingItemId, cancellationToken);

        if (item is null)
        {
            throw new NotFoundException($"Reading item '{readingItemId}' was not found.");
        }

        if (!ReadingItemAccessPolicy.IsOwner(item, username))
        {
            throw new ForbiddenException("You do not have access to this reading item.");
        }

        return item;
    }

    public async Task<string?> ResolveOriginalLanguageAsync(
        Guid? readingItemId,
        CancellationToken cancellationToken)
    {
        if (!readingItemId.HasValue)
        {
            return null;
        }

        return await dbContext.ReadingItems
            .Where(item => item.Id == readingItemId.Value)
            .Select(item => item.OriginalLanguage)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
