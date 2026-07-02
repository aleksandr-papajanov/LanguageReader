using LanguageReader.Infrastructure.Exceptions;
using LanguageReader.Infrastructure.Features.Reading.Services;

namespace LanguageReader.Api.Features.Reading;

internal sealed class SaveReadingProgressHandler(ReadingProgressService readingProgress)
{
    public async Task<ReadingProgressDto> HandleAsync(SaveReadingProgressRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);

        if (request.Position.ReadingItemId != request.ReadingItemId)
        {
            throw new ValidationException("Reading item position must match the requested reading item.");
        }

        var progress = await readingProgress.SaveAsync(
            normalizedUsername,
            request.ReadingItemId,
            request.ProgressPercent,
            request.Position,
            ct);

        return progress.ToReadingProgressDto();
    }
}
