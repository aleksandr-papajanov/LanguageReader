using LanguageReader.Infrastructure.Features.ReadingItems.Entities;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public interface IReadingItemContentService
{
    Task<ReadingItemContentPageDto> LoadPageAsync(
        ReadingItemEntity item,
        GetReadingItemContentRequest request,
        CancellationToken cancellationToken = default);
}
