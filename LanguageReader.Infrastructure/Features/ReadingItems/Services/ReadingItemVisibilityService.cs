using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public sealed class ReadingItemVisibilityService(ApplicationDbContext dbContext)
{
    public async Task UpdateAsync(
        ReadingItemEntity item,
        bool isPublic,
        CancellationToken cancellationToken)
    {
        item.IsPublic = isPublic;
        item.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
