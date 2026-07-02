using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Services;

public sealed class VocabularyEntryDeletionService(ApplicationDbContext dbContext)
{
    public async Task DeleteAsync(
        VocabularyEntryEntity entry,
        CancellationToken cancellationToken)
    {
        dbContext.VocabularyEntries.Remove(entry);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
