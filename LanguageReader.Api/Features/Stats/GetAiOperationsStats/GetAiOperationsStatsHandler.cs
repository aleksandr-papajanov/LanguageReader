using LanguageReader.Api.Features.Common.Mapping;
using LanguageReader.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Stats;

internal sealed class GetAiOperationsStatsHandler(ApplicationDbContext dbContext)
{
    public async Task<GetAiOperationsStatsResponse> HandleAsync(
        GetAiOperationsStatsRequest request,
        CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);

        var query = dbContext.AiOperations
            .AsNoTracking()
            .Where(operation => operation.Username == normalizedUsername);

        if (request.FromUtc.HasValue)
        {
            query = query.Where(operation => operation.CreatedAtUtc >= request.FromUtc.Value.ToUniversalTime());
        }

        if (request.ToUtc.HasValue)
        {
            query = query.Where(operation => operation.CreatedAtUtc < request.ToUtc.Value.ToUniversalTime());
        }

        var operations = await query
            .OrderByDescending(operation => operation.CreatedAtUtc)
            .ToListAsync(ct);

        return new GetAiOperationsStatsResponse(
            operations.ToAiUsageSummaryDto(),
            operations.Select(operation => operation.ToAiOperationDto()).ToList());
    }
}
