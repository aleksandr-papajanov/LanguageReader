using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.ReadingItems.Services;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class GetReadingItemContentHandler(
    ApplicationDbContext dbContext,
    IReadingItemContentService readingItemContentService)
{
    public async Task<ReadingItemContentDto> HandleAsync(GetReadingItemContentRequest request, CancellationToken ct)
    {
        var item = await dbContext.ReadingItems
            .AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == request.ReadingItemId, ct);

        if (item is null)
        {
            throw new NotFoundException($"Reading item '{request.ReadingItemId}' was not found.");
        }

        if (!ReadingItemFeatureHelpers.CanRead(item, request.Username))
        {
            throw new ForbiddenException("You do not have access to this reading item.");
        }

        return await readingItemContentService.LoadAsync(item, ct);
    }
}
