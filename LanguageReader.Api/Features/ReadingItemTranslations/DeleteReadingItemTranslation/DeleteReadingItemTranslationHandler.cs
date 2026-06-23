using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.ReadingItemTranslations;

internal sealed class DeleteReadingItemTranslationHandler(ApplicationDbContext dbContext)
{
    public async Task HandleAsync(DeleteReadingItemTranslationRequest request, CancellationToken ct)
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

        await dbContext.AiOperations
            .Where(operation => operation.TranslatedRangeId == range.Id && operation.VocabularyEntryId == null)
            .ExecuteDeleteAsync(ct);

        dbContext.TranslatedRanges.Remove(range);
        await dbContext.SaveChangesAsync(ct);
    }
}

