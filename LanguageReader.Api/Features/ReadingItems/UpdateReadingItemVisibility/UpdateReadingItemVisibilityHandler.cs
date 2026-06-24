using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class UpdateReadingItemVisibilityHandler(ApplicationDbContext dbContext)
{
    public async Task HandleAsync(UpdateReadingItemVisibilityRequest request, CancellationToken ct)
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

        item.IsPublic = request.IsPublic;
        item.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);
    }
}
