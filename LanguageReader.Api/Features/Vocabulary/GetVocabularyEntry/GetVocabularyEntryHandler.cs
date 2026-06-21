using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class GetVocabularyEntryHandler(ApplicationDbContext dbContext)
{
    public async Task<VocabularyEntryDto> HandleAsync(GetVocabularyEntryRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var entry = await dbContext.VocabularyEntries
            .AsNoTracking()
            .Include(item => item.Book)
            .Include(item => item.WordDetails)
            .Include(item => item.RelatedWords)
            .Include(item => item.AiOperations)
            .Include(item => item.Examples)
                .ThenInclude(example => example.Book)
            .FirstOrDefaultAsync(item => item.Id == request.VocabularyId && item.Username == normalizedUsername, ct);

        if (entry is null)
        {
            throw new NotFoundException($"Vocabulary entry '{request.VocabularyId}' was not found.");
        }

        return entry.ToVocabularyEntryDto();
    }
}
