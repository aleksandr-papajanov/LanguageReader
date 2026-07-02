using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;

namespace LanguageReader.Infrastructure.Features.Vocabulary.Services;

public sealed class VocabularyVisibilityService(ApplicationDbContext dbContext)
{
    public async Task UpdateAsync(
        VocabularyEntryEntity entry,
        bool isVisible,
        CancellationToken cancellationToken)
    {
        entry.IsVisibleInVocabulary = isVisible;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
