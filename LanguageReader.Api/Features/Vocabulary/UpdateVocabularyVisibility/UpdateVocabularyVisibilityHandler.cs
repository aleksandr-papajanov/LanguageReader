using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class UpdateVocabularyVisibilityHandler(ApplicationDbContext dbContext)
{
    public async Task<VocabularyEntryDto> HandleAsync(UpdateVocabularyVisibilityRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var entry = await dbContext.VocabularyEntries
            .Include(entry => entry.Book)
            .Include(entry => entry.WordDetails)
            .Include(entry => entry.RelatedWords)
            .Include(entry => entry.AiOperations)
            .Include(entry => entry.Examples)
                .ThenInclude(example => example.Book)
            .FirstOrDefaultAsync(
            entry => entry.Id == request.VocabularyId && entry.Username == normalizedUsername,
            ct);

        if (entry is null)
        {
            throw new NotFoundException($"Vocabulary entry '{request.VocabularyId}' was not found.");
        }

        if (entry.Book is null)
        {
            throw new NotFoundException($"Book for vocabulary entry '{request.VocabularyId}' was not found.");
        }

        entry.IsVisibleInVocabulary = request.IsVisibleInVocabulary;
        await dbContext.SaveChangesAsync(ct);

        return entry.ToVocabularyEntryDto();
    }
}
