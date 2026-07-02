using LanguageReader.Infrastructure.Features.ReadingItems.Services;

namespace LanguageReader.Api.Features.ReadingItems;

internal sealed class UpdateReadingItemVisibilityHandler(
    ReadingItemAccessService readingItems,
    ReadingItemVisibilityService visibility)
{
    public async Task HandleAsync(UpdateReadingItemVisibilityRequest request, CancellationToken ct)
    {
        var username = UsernameHelper.Require(request.Username);
        var item = await readingItems.LoadOwnedAsync(request.ReadingItemId, username, ct);

        await visibility.UpdateAsync(item, request.IsPublic, ct);
    }
}
