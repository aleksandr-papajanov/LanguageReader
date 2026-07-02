using LanguageReader.Infrastructure.Features.Stats.Services;

namespace LanguageReader.Api.Features.Stats;

internal sealed class GetAiOperationsStatsHandler(AiOperationsStatsService stats)
{
    public async Task<GetAiOperationsStatsResponse> HandleAsync(
        GetAiOperationsStatsRequest request,
        CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var operations = await stats.LoadAsync(normalizedUsername, request.FromUtc, request.ToUtc, ct);

        return operations.ToGetAiOperationsStatsResponse();
    }
}
