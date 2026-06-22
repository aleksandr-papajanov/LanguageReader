namespace LanguageReader.Client.Features.Stats.Services;

public sealed class StatsApiClient(ApiClient api)
{
    public Task<GetAiOperationsStatsResponse> GetAiOperationsStatsAsync(
        GetAiOperationsStatsRequest request,
        CancellationToken cancellationToken = default)
    {
        return api.GetAsync<GetAiOperationsStatsResponse>(
            "/api/stats/ai-operations",
            request,
            cancellationToken);
    }
}
