using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Features.Ai.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Infrastructure.Features.Stats.Services;

public sealed class AiOperationsStatsService(ApplicationDbContext dbContext)
{
    public async Task<IReadOnlyList<AiOperationEntity>> LoadAsync(
        string normalizedUsername,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AiOperations
            .AsNoTracking()
            .Where(operation => operation.Username == normalizedUsername);

        if (fromUtc.HasValue)
        {
            query = query.Where(operation => operation.CreatedAtUtc >= fromUtc.Value.ToUniversalTime());
        }

        if (toUtc.HasValue)
        {
            query = query.Where(operation => operation.CreatedAtUtc < toUtc.Value.ToUniversalTime());
        }

        return await query
            .OrderByDescending(operation => operation.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
