using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.ReadingItems.EntityConfigurations;

internal sealed class ArticleMetadataConfiguration : IEntityTypeConfiguration<ArticleMetadataEntity>
{
    public void Configure(EntityTypeBuilder<ArticleMetadataEntity> entity)
    {
        entity.ToTable("article_metadata");
        entity.HasKey(item => item.ReadingItemId);

        entity.Property(item => item.ReadingItemId).HasColumnName("reading_item_id");
        entity.Property(item => item.SourceName).HasColumnName("source_name").HasMaxLength(256).IsRequired();
        entity.Property(item => item.OriginalUrl).HasColumnName("original_url").HasMaxLength(2048).IsRequired();
        entity.Property(item => item.PublishedAtUtc).HasColumnName("published_at_utc");
        entity.Property(item => item.Author).HasColumnName("author").HasMaxLength(512);
        entity.Property(item => item.ImageUrl).HasColumnName("image_url").HasMaxLength(2048);
        entity.Property(item => item.Excerpt).HasColumnName("excerpt").HasMaxLength(4096);
        entity.Property(item => item.RssFeedUrl).HasColumnName("rss_feed_url").HasMaxLength(2048);
        entity.Property(item => item.ExternalId).HasColumnName("external_id").HasMaxLength(1024);

        entity.HasIndex(item => item.ExternalId);

        entity.HasOne(item => item.ReadingItem)
            .WithOne(item => item.ArticleMetadata)
            .HasForeignKey<ArticleMetadataEntity>(item => item.ReadingItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
