using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Ai;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Services;

public sealed class VocabularyEntrySaveService(ApplicationDbContext dbContext)
{
    public async Task SaveAsync(
        VocabularyEntryEntity entry,
        bool isNewEntry,
        AiOperationUsageDto? normalizationUsage,
        CancellationToken cancellationToken)
    {
        if (isNewEntry)
        {
            dbContext.VocabularyEntries.Add(entry);
        }

        if (normalizationUsage is not null)
        {
            dbContext.AiOperations.Add(AiOperationMapper.ToEntity(
                normalizationUsage,
                entry.Username,
                vocabularyEntryId: entry.Id));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
