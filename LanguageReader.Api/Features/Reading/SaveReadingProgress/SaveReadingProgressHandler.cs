using LanguageReader.Infrastructure.Data;
using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Reading.Entities;
using Microsoft.EntityFrameworkCore;

namespace LanguageReader.Api.Features.Reading;

internal sealed class SaveReadingProgressHandler(ApplicationDbContext dbContext)
{
    public async Task<ReadingProgressDto> HandleAsync(SaveReadingProgressRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);

        if (request.Position.ReadingItemId != request.ReadingItemId)
        {
            throw new ValidationException("Reading item position must match the requested reading item.");
        }

        var progressPercent = Math.Clamp(request.ProgressPercent, 0, 100);
        var progress = await dbContext.ReadingProgresses
            .FirstOrDefaultAsync(progress =>
                progress.Username == normalizedUsername
                && progress.ReadingItemId == request.ReadingItemId,
                ct);

        if (progress is null)
        {
            progress = new ReadingProgressEntity
            {
                Id = Guid.NewGuid(),
                Username = normalizedUsername,
                ReadingItemId = request.ReadingItemId
            };
            dbContext.ReadingProgresses.Add(progress);
        }

        progress.ProgressPercent = progressPercent;
        progress.BlockIndex = Math.Max(0, request.Position.BlockIndex);
        progress.CharacterOffset = Math.Max(0, request.Position.CharacterOffset);
        progress.LastOpenedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(ct);

        return progress.ToReadingProgressDto();
    }
}
