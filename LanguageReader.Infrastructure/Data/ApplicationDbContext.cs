using Microsoft.EntityFrameworkCore;
using LanguageReader.Infrastructure.Features.Ai.Entities;
using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using LanguageReader.Infrastructure.Features.Books.Entities;
using LanguageReader.Infrastructure.Features.Reading.Entities;
using LanguageReader.Infrastructure.Features.ReadingItemTranslations.Entities;
using LanguageReader.Infrastructure.Features.Settings.Entities;
using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using LanguageReader.Infrastructure.Features.News.Entities;
using LanguageReader.Infrastructure.Features.Users.Entities;

namespace LanguageReader.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the application.
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Stored book metadata.
    /// </summary>
    public DbSet<BookEntity> Books => Set<BookEntity>();

    /// <summary>
    /// Stored generic reading items.
    /// </summary>
    public DbSet<ReadingItemEntity> ReadingItems => Set<ReadingItemEntity>();

    /// <summary>
    /// Stored article metadata for reading items.
    /// </summary>
    public DbSet<ArticleMetadataEntity> ArticleMetadata => Set<ArticleMetadataEntity>();

    /// <summary>
    /// Stored reading progress.
    /// </summary>
    public DbSet<ReadingProgressEntity> ReadingProgresses => Set<ReadingProgressEntity>();

    /// <summary>
    /// Stored AI operation metadata.
    /// </summary>
    public DbSet<AiOperationEntity> AiOperations => Set<AiOperationEntity>();

    /// <summary>
    /// Stored user settings.
    /// </summary>
    public DbSet<UserSettingsEntity> UserSettings => Set<UserSettingsEntity>();

    /// <summary>
    /// Stored vocabulary entries.
    /// </summary>
    public DbSet<VocabularyEntryEntity> VocabularyEntries => Set<VocabularyEntryEntity>();

    /// <summary>
    /// Stored vocabulary word details.
    /// </summary>
    public DbSet<VocabularyWordDetailsEntity> VocabularyWordDetails => Set<VocabularyWordDetailsEntity>();

    /// <summary>
    /// Stored vocabulary examples.
    /// </summary>
    public DbSet<VocabularyExampleEntity> VocabularyExamples => Set<VocabularyExampleEntity>();

    /// <summary>
    /// Stored related words for vocabulary entries.
    /// </summary>
    public DbSet<RelatedWordEntity> RelatedWords => Set<RelatedWordEntity>();

    /// <summary>
    /// Stored translated ranges.
    /// </summary>
    public DbSet<TranslatedRangeEntity> TranslatedRanges => Set<TranslatedRangeEntity>();

    /// <summary>
    /// Stored RSS article candidates.
    /// </summary>
    public DbSet<RssArticleCandidateEntity> RssArticleCandidates => Set<RssArticleCandidateEntity>();

    /// <summary>
    /// Registered user accounts.
    /// </summary>
    public DbSet<UserAccountEntity> UserAccounts => Set<UserAccountEntity>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

