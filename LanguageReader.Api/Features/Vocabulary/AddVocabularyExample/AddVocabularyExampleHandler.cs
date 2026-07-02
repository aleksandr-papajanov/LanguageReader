using LanguageReader.Infrastructure.Ai.Workflows;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Services;
using LanguageReader.Infrastructure.Features.Vocabulary.Workflows;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class AddVocabularyExampleHandler(
    VocabularyEntryGraphService vocabularyEntries,
    WorkflowRunner workflowRunner)
{
    public async Task<VocabularyEntryDto> HandleAsync(
        AddVocabularyExampleRequest request,
        CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var entry = await vocabularyEntries.LoadOwnedAsync(request.VocabularyId, normalizedUsername, ct);

        if (entry.Kind != SavedTextKind.LexicalUnit)
        {
            throw new ValidationException("Generated usage examples are only available for saved words.");
        }

        var updatedEntry = await workflowRunner.RunAsync<AddVocabularyExampleWorkflow, AddVocabularyExampleWorkflowRequest, VocabularyEntryEntity>(
            new AddVocabularyExampleWorkflowRequest(entry),
            ct);

        return updatedEntry.ToVocabularyEntryDto();
    }
}
