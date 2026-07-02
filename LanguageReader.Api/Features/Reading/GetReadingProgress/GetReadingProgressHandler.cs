using LanguageReader.Infrastructure.Features.Reading.Services;

namespace LanguageReader.Api.Features.Reading;

internal sealed class GetReadingProgressHandler(ReadingProgressService readingProgress)
{
    public async Task<ReadingProgressDto> HandleAsync(GetReadingProgressRequest request, CancellationToken ct)
    {
        var normalizedUsername = UsernameHelper.Require(request.Username);
        var progress = await readingProgress.LoadReadOnlyAsync(normalizedUsername, request.ReadingItemId, ct);

        return progress is null
            ? request.ToEmptyReadingProgressDto(normalizedUsername)
            : progress.ToReadingProgressDto();
    }
}
