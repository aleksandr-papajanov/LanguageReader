using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.BookTranslations;

internal sealed class UpdateBookTranslationDisplayHandler(ApplicationDbContext dbContext)
{
    public async Task<TranslatedRangeDto> HandleAsync(
        UpdateTranslatedRangeDisplayRequest request,
        CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var range = await dbContext.TranslatedRanges.FirstOrDefaultAsync(
            range =>
                range.Id == request.TranslationId
                && range.ReadingItemId == request.ReadingItemId
                && range.Username == normalizedUsername,
            ct);

        if (range is null)
        {
            throw new NotFoundException($"Translated range '{request.TranslationId}' was not found.");
        }

        range.ShowOriginal = request.ShowOriginal;
        await dbContext.SaveChangesAsync(ct);

        return range.ToTranslatedRangeDto();
    }
}
