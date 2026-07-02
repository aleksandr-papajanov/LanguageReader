using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Services;

public sealed class VocabularyExampleDeletionService(ApplicationDbContext dbContext)
{
    public async Task DeleteAsync(
        Guid vocabularyEntryId,
        Guid exampleId,
        CancellationToken cancellationToken)
    {
        var example = await dbContext.VocabularyExamples
            .FirstOrDefaultAsync(
                item => item.Id == exampleId && item.VocabularyEntryId == vocabularyEntryId,
                cancellationToken);

        if (example is null)
        {
            throw new NotFoundException($"Vocabulary example '{exampleId}' was not found.");
        }

        if (example.IsFromReadingItem)
        {
            throw new ValidationException("Examples from the book cannot be deleted.");
        }

        dbContext.VocabularyExamples.Remove(example);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
