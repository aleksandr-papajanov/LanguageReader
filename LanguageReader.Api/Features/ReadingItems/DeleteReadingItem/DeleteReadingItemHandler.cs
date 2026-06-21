using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class DeleteReadingItemHandler(
    ApplicationDbContext dbContext,
    IFileStorage storage)
{
    public async Task HandleAsync(DeleteReadingItemRequest request, CancellationToken ct)
    {
        var username = UsernameHelper.Require(request.Username);
        var item = await dbContext.ReadingItems.FirstOrDefaultAsync(candidate => candidate.Id == request.ReadingItemId, ct);
        if (item is null)
        {
            throw new NotFoundException($"Reading item '{request.ReadingItemId}' was not found.");
        }

        if (!string.Equals(item.OwnerUsername, username, StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("You do not have access to this reading item.");
        }

        var book = await dbContext.Books.FirstOrDefaultAsync(candidate => candidate.Id == request.ReadingItemId, ct);
        if (book is not null)
        {
            dbContext.Books.Remove(book);
        }

        var rssCandidates = await dbContext.RssArticleCandidates
            .Where(candidate => candidate.SavedReadingItemId == request.ReadingItemId)
            .ToListAsync(ct);

        foreach (var candidate in rssCandidates)
        {
            candidate.SavedReadingItemId = null;
            candidate.Status = NewsArticleStatus.ExtractionSucceeded;
            candidate.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        dbContext.ReadingItems.Remove(item);
        await dbContext.SaveChangesAsync(ct);
        await storage.DeleteAsync(item.StoragePath, ct);
    }
}
