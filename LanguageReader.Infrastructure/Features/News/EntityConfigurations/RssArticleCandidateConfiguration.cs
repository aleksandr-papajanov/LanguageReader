using LanguageReader.Infrastructure.Features.News.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.News.EntityConfigurations;

internal sealed class RssArticleCandidateConfiguration : IEntityTypeConfiguration<RssArticleCandidateEntity>
{
    public void Configure(EntityTypeBuilder<RssArticleCandidateEntity> entity)
    {
        entity.ToTable("rss_article_candidates");
        entity.HasKey(item => item.Id);

        entity.Property(item => item.Id).HasColumnName("id");
        entity.Property(item => item.SourceKey).HasColumnName("source_key").HasMaxLength(128).IsRequired();
        entity.Property(item => item.SourceName).HasColumnName("source_name").HasMaxLength(256).IsRequired();
        entity.Property(item => item.Title).HasColumnName("title").HasMaxLength(1024).IsRequired();
        entity.Property(item => item.Url).HasColumnName("url").HasMaxLength(2048).IsRequired();
        entity.Property(item => item.ExternalId).HasColumnName("external_id").HasMaxLength(1024);
        entity.Property(item => item.PublishedAtUtc).HasColumnName("published_at_utc");
        entity.Property(item => item.Summary).HasColumnName("summary").HasMaxLength(4096);
        entity.Property(item => item.Author).HasColumnName("author").HasMaxLength(512);
        entity.Property(item => item.ImageUrl).HasColumnName("image_url").HasMaxLength(2048);
        entity.Property(item => item.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(32).IsRequired();
        entity.Property(item => item.SavedReadingItemId).HasColumnName("saved_reading_item_id");
        entity.Property(item => item.CreatedAtUtc).HasColumnName("created_at_utc");
        entity.Property(item => item.UpdatedAtUtc).HasColumnName("updated_at_utc");

        entity.HasIndex(item => item.SourceKey);
        entity.HasIndex(item => new { item.SourceKey, item.Url }).IsUnique();
        entity.HasIndex(item => new { item.SourceKey, item.ExternalId });
    }
}
