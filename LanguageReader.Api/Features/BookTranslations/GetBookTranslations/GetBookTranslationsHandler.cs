using LanguageReader.Api.Features.ReadingItems;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.BookTranslations;

internal sealed class GetBookTranslationsHandler(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyList<TranslatedRangeDto>> HandleAsync(GetBookTranslationsRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var readingItem = await dbContext.ReadingItems.AsNoTracking().FirstOrDefaultAsync(item => item.Id == request.ReadingItemId, ct);
        if (readingItem is null)
        {
            throw new NotFoundException($"Reading item '{request.ReadingItemId}' was not found.");
        }

        if (!ReadingItemFeatureHelpers.CanRead(readingItem, normalizedUsername))
        {
            throw new ForbiddenException("You do not have access to this reading item.");
        }

        var translations = await dbContext.TranslatedRanges
            .AsNoTracking()
            .Where(range => range.Username == normalizedUsername && range.ReadingItemId == request.ReadingItemId)
            .OrderBy(range => range.ParagraphIndex)
            .ThenBy(range => range.StartOffset)
            .ToListAsync(ct);

        return translations.Select(range => range.ToTranslatedRangeDto()).ToList();
    }
}
