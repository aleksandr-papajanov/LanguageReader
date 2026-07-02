using LanguageReader.Infrastructure.Features.ReadingItems.Services;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class DeleteReadingItemHandler(
    ReadingItemAccessService readingItems,
    ReadingItemDeletionService deletion)
{
    public async Task HandleAsync(DeleteReadingItemRequest request, CancellationToken ct)
    {
        var username = UsernameHelper.Require(request.Username);
        var item = await readingItems.LoadOwnedAsync(request.ReadingItemId, username, ct);
        await deletion.DeleteAsync(item, ct);
    }
}
