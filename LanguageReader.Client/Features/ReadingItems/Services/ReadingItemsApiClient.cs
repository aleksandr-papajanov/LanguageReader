namespace LanguageReader.Client.Features.ReadingItems.Services;

public sealed class ReadingItemsApiClient(ApiClient api)
{
    public Task<IReadOnlyList<ReadingItemSummaryDto>> GetReadingItemsAsync(
        GetReadingItemsRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.GetAsync<IReadOnlyList<ReadingItemSummaryDto>>(
            "/api/reading-items",
            request,
            cancellationToken);
    }

    public Task<ReadingItemDetailsDto> GetReadingItemAsync(
        GetReadingItemRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.GetAsync<ReadingItemDetailsDto>(
            "/api/reading-items/{ReadingItemId}",
            request,
            cancellationToken);
    }

    public Task<ReadingItemContentDto> GetReadingItemContentAsync(
        GetReadingItemContentRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.GetAsync<ReadingItemContentDto>(
            "/api/reading-items/{ReadingItemId}/content",
            request,
            cancellationToken);
    }

    public Task DeleteReadingItemAsync(
        DeleteReadingItemRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.DeleteAsync(
            "/api/reading-items/{ReadingItemId}",
            request,
            cancellationToken);
    }

    public Task SetVisibilityAsync(
        UpdateReadingItemVisibilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var route = new UpdateReadingItemVisibilityRequestRoute(request.ReadingItemId);
        var body = new UpdateReadingItemVisibilityRequestBody(request.Username, request.IsPublic);

        return api.PutAsync(
            "/api/reading-items/{ReadingItemId}/visibility",
            route,
            body,
            cancellationToken);
    }
}
