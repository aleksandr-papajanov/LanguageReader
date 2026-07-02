using LanguageReader.Infrastructure.Features.Reading.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Models;

public sealed record ReadingItemLibraryQueryResult(
    ReadingItemEntity Item,
    ReadingProgressEntity? Progress);
