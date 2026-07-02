using LanguageReader.Infrastructure.Features.Vocabulary.Services;

namespace LanguageReader.Api.Features.Vocabulary;

internal sealed class DeleteVocabularyEntryHandler(
    VocabularyEntryGraphService vocabularyEntries,
    VocabularyEntryDeletionService deletion)
{
    public async Task HandleAsync(DeleteVocabularyEntryRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);

        var entry = await vocabularyEntries.LoadOwnedAsync(request.VocabularyId, normalizedUsername, ct);
        await deletion.DeleteAsync(entry, ct);
    }
}
