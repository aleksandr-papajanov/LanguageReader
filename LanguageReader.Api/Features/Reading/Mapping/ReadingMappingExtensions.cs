using LanguageReader.Infrastructure.Features.Reading.Entities;
using LanguageReader.Api.Features.ReadingItems;

namespace LanguageReader.Api.Features.Reading;

internal static class ReadingMappingExtensions
{
    public static ReadingProgressDto ToReadingProgressDto(this ReadingProgressEntity progress)
    {
        return new ReadingProgressDto(
            progress.ReadingItemId,
            progress.Username,
            progress.ProgressPercent,
            progress.LastOpenedAtUtc,
            new ReadingPositionDto(progress.ReadingItemId, progress.ParagraphIndex, progress.CharacterOffset));
    }

    public static ReadingProgressDto ToEmptyReadingProgressDto(this GetReadingProgressRequest request, string username)
    {
        return new ReadingProgressDto(
            request.ReadingItemId,
            username,
            0,
            DateTimeOffset.UtcNow,
            new ReadingPositionDto(request.ReadingItemId, 0, 0));
    }
}
