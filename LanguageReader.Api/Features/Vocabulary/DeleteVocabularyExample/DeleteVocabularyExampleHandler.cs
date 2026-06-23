using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class DeleteVocabularyExampleHandler(
    ApplicationDbContext dbContext)
{
    public async Task<VocabularyEntryDto> HandleAsync(
        DeleteVocabularyExampleRequest request,
        CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);

        var entry = await dbContext.VocabularyEntries
            .FirstOrDefaultAsync(
                item => item.Id == request.VocabularyId && item.Username == normalizedUsername,
                ct);

        if (entry is null)
        {
            throw new NotFoundException($"Vocabulary entry '{request.VocabularyId}' was not found.");
        }

        var example = await dbContext.VocabularyExamples
            .FirstOrDefaultAsync(
                item => item.Id == request.ExampleId && item.VocabularyEntryId == request.VocabularyId,
                ct);

        if (example is null)
        {
            throw new NotFoundException($"Vocabulary example '{request.ExampleId}' was not found.");
        }

        if (example.IsFromReadingItem)
        {
            throw new ValidationException("Examples from the book cannot be deleted.");
        }

        dbContext.VocabularyExamples.Remove(example);
        await dbContext.SaveChangesAsync(ct);

        var updatedEntry = await dbContext.VocabularyEntries
            .Include(item => item.ReadingItem)
            .Include(item => item.WordDetails)
            .Include(item => item.RelatedWords)
            .Include(item => item.AiOperations)
            .Include(item => item.Examples)
                .ThenInclude(item => item.ReadingItem)
            .FirstAsync(
                item => item.Id == request.VocabularyId && item.Username == normalizedUsername,
                ct);

        return updatedEntry.ToVocabularyEntryDto();
    }
}
