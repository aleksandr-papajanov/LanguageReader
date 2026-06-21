using LanguageReader.Infrastructure.Features.ReadingItems.Entities;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Services;

public interface IReadingItemContentService
{
    Task<ReadingItemContentDto> LoadAsync(ReadingItemEntity item, CancellationToken cancellationToken = default);
}
