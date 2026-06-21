using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.ReadingItems.EntityConfigurations;

internal sealed class ReadingItemConfiguration : IEntityTypeConfiguration<ReadingItemEntity>
{
    public void Configure(EntityTypeBuilder<ReadingItemEntity> entity)
    {
        entity.ToTable("reading_items");
        entity.HasKey(item => item.Id);

        entity.Property(item => item.Id).HasColumnName("id");
        entity.Property(item => item.OwnerUsername).HasColumnName("owner_username").HasMaxLength(128).IsRequired();
        entity.Property(item => item.Title).HasColumnName("title").HasMaxLength(512).IsRequired();
        entity.Property(item => item.OriginalLanguage).HasColumnName("original_language").HasMaxLength(64).IsRequired();
        entity.Property(item => item.StoragePath).HasColumnName("storage_path").HasMaxLength(1024).IsRequired();
        entity.Property(item => item.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(32).IsRequired();
        entity.Property(item => item.ContentFormat).HasColumnName("content_format").HasConversion<string>().HasMaxLength(32).IsRequired();
        entity.Property(item => item.IsPublic).HasColumnName("is_public");
        entity.Property(item => item.CreatedAtUtc).HasColumnName("created_at_utc");
        entity.Property(item => item.UpdatedAtUtc).HasColumnName("updated_at_utc");

        entity.HasIndex(item => item.OwnerUsername);
        entity.HasIndex(item => item.IsPublic);
        entity.HasIndex(item => item.Type);
    }
}
