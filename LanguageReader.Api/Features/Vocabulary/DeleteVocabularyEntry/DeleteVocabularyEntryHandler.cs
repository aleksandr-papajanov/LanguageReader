using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class DeleteVocabularyEntryHandler(ApplicationDbContext dbContext)
{
    public async Task HandleAsync(DeleteVocabularyEntryRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);

        var entry = await dbContext.VocabularyEntries
            .FirstOrDefaultAsync(item => item.Id == request.VocabularyId && item.Username == normalizedUsername, ct);

        if (entry is null)
        {
            throw new NotFoundException($"Vocabulary entry '{request.VocabularyId}' was not found.");
        }

        dbContext.VocabularyEntries.Remove(entry);
        await dbContext.SaveChangesAsync(ct);
    }
}
