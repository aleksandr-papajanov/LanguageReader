using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.ReadingItems.EntityConfigurations;

internal sealed class ReadingItemAssetConfiguration : IEntityTypeConfiguration<ReadingItemAssetEntity>
{
    public void Configure(EntityTypeBuilder<ReadingItemAssetEntity> entity)
    {
        entity.ToTable("reading_item_assets");
        entity.HasKey(item => new { item.ReadingItemId, item.AssetId });

        entity.Property(item => item.ReadingItemId).HasColumnName("reading_item_id");
        entity.Property(item => item.AssetId).HasColumnName("asset_id").HasMaxLength(512);
        entity.Property(item => item.Kind).HasColumnName("kind").HasMaxLength(32).IsRequired();
        entity.Property(item => item.ContentType).HasColumnName("content_type").HasMaxLength(128).IsRequired();
        entity.Property(item => item.StoragePath).HasColumnName("storage_path").HasMaxLength(1024).IsRequired();
        entity.Property(item => item.AltText).HasColumnName("alt_text").HasMaxLength(1024);
        entity.Property(item => item.Width).HasColumnName("width");
        entity.Property(item => item.Height).HasColumnName("height");
        entity.Property(item => item.IsCover).HasColumnName("is_cover");

        entity.HasIndex(item => new { item.ReadingItemId, item.IsCover });

        entity.HasOne(item => item.ReadingItem)
            .WithMany(item => item.Assets)
            .HasForeignKey(item => item.ReadingItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
