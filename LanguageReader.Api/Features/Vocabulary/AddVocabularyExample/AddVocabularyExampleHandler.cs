using LanguageReader.Infrastructure.Ai.Workflows;
using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Workflows;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class AddVocabularyExampleHandler(
    ApplicationDbContext dbContext,
    WorkflowRunner workflowRunner)
{
    public async Task<VocabularyEntryDto> HandleAsync(
        AddVocabularyExampleRequest request,
        CancellationToken ct)
    {
        var entry = await LoadOwnedEntryAsync(request.VocabularyId, request.Username, ct);

        if (entry.Kind != SavedTextKind.LexicalUnit)
        {
            throw new ValidationException("Generated usage examples are only available for saved words.");
        }

        var updatedEntry = await workflowRunner.RunAsync<AddVocabularyExampleWorkflow, AddVocabularyExampleWorkflowRequest, VocabularyEntryEntity>(
            new AddVocabularyExampleWorkflowRequest(entry),
            ct);

        return updatedEntry.ToVocabularyEntryDto();
    }

    private async Task<VocabularyEntryEntity> LoadOwnedEntryAsync(Guid id, string username, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(username);
        var entry = await dbContext.VocabularyEntries
            .Include(item => item.ReadingItem)
            .Include(item => item.WordDetails)
            .Include(item => item.RelatedWords)
            .Include(item => item.AiOperations)
            .Include(item => item.Examples)
                .ThenInclude(example => example.ReadingItem)
            .FirstOrDefaultAsync(item => item.Id == id && item.Username == normalizedUsername, ct);

        if (entry is null)
        {
            throw new NotFoundException($"Vocabulary entry '{id}' was not found.");
        }

        return entry;
    }
}
