using LanguageReader.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Reading;

internal sealed class GetReadingProgressHandler(ApplicationDbContext dbContext)
{
    public async Task<ReadingProgressDto> HandleAsync(GetReadingProgressRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var progress = await dbContext.ReadingProgresses
            .AsNoTracking()
            .FirstOrDefaultAsync(progress =>
                progress.Username == normalizedUsername
                && progress.ReadingItemId == request.ReadingItemId,
                ct);

        return progress is null
            ? request.ToEmptyReadingProgressDto(normalizedUsername)
            : progress.ToReadingProgressDto();
    }
}
