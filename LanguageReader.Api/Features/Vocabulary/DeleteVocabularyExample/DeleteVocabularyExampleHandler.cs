using LanguageReader.Infrastructure.Features.Vocabulary.Services;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class DeleteVocabularyExampleHandler(
    VocabularyEntryGraphService vocabularyEntries,
    VocabularyExampleDeletionService deletion)
{
    public async Task<VocabularyEntryDto> HandleAsync(
        DeleteVocabularyExampleRequest request,
        CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        _ = await vocabularyEntries.LoadOwnedAsync(request.VocabularyId, normalizedUsername, ct);

        await deletion.DeleteAsync(request.VocabularyId, request.ExampleId, ct);
        var updatedEntry = await vocabularyEntries.LoadOwnedAsync(request.VocabularyId, normalizedUsername, ct);

        return updatedEntry.ToVocabularyEntryDto();
    }
}
