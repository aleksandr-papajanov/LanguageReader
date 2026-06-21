namespace LanguageReader.Client.Features.Reading.Services;

public sealed class ReadingApiClient(ApiClient api)
{
    public Task<ReadingProgressDto> GetReadingProgressAsync(
        GetReadingProgressRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.GetAsync<ReadingProgressDto>(
            "/api/reading-items/{ReadingItemId}/progress",
            request,
            cancellationToken);
    }

    public Task SaveReadingProgressAsync(
        SaveReadingProgressRequest request,
        CancellationToken cancellationToken = default)
    {
        var route = new SaveReadingProgressRequestRoute(request.ReadingItemId);
        var body = new SaveReadingProgressRequestBody(
            request.Username,
            request.ProgressPercent,
            request.Position);

        return api.PutAsync(
            "/api/reading-items/{ReadingItemId}/progress",
            route,
            body,
            cancellationToken);
    }
}
