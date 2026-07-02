using LanguageReader.Infrastructure.Features.News.Entities;
using LanguageReader.Infrastructure.Features.Reading.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;

namespace LanguageReader.Infrastructure.Features.ReadingItems.Models;

public sealed record ReadingItemDiscoveryQueryResult(
    RssArticleCandidateEntity Candidate,
    ReadingItemEntity? SavedItem,
    ReadingProgressEntity? Progress);
