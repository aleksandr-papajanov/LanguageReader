using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.ReadingItems.EntityConfigurations;

internal sealed class ReadingItemBlockConfiguration : IEntityTypeConfiguration<ReadingItemBlockEntity>
{
    public void Configure(EntityTypeBuilder<ReadingItemBlockEntity> entity)
    {
        entity.ToTable("reading_item_blocks");
        entity.HasKey(item => new { item.ReadingItemId, item.SequenceIndex });

        entity.Property(item => item.ReadingItemId).HasColumnName("reading_item_id");
        entity.Property(item => item.SequenceIndex).HasColumnName("sequence_index");
        entity.Property(item => item.BlockIndex).HasColumnName("block_index");
        entity.Property(item => item.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(32).IsRequired();
        entity.Property(item => item.Text).HasColumnName("text");
        entity.Property(item => item.ImageId).HasColumnName("image_id").HasMaxLength(512);
        entity.Property(item => item.Weight).HasColumnName("weight").IsRequired();

        entity.HasIndex(item => new { item.ReadingItemId, item.BlockIndex });

        entity.HasOne(item => item.ReadingItem)
            .WithMany(item => item.Blocks)
            .HasForeignKey(item => item.ReadingItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
