using LanguageReader.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class GetVocabularyHandler(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyList<VocabularyEntryDto>> HandleAsync(GetVocabularyRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var rows = await dbContext.VocabularyEntries
            .AsNoTracking()
            .Include(entry => entry.ReadingItem)
            .Include(entry => entry.WordDetails)
            .Include(entry => entry.RelatedWords)
            .Include(entry => entry.AiOperations)
            .Include(entry => entry.Examples)
                .ThenInclude(example => example.ReadingItem)
            .Where(entry => entry.Username == normalizedUsername && (request.IncludeHidden == true || entry.IsVisibleInVocabulary))
            .OrderByDescending(row => row.CreatedAtUtc)
            .ToListAsync(ct);

        return rows.Select(entry => entry.ToVocabularyEntryDto()).ToList();
    }
}
