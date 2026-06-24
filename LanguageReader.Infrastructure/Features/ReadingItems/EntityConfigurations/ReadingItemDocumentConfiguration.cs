using LanguageReader.Infrastructure.Features.ReadingItems.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.ReadingItems.EntityConfigurations;

internal sealed class ReadingItemDocumentConfiguration : IEntityTypeConfiguration<ReadingItemDocumentEntity>
{
    public void Configure(EntityTypeBuilder<ReadingItemDocumentEntity> entity)
    {
        entity.ToTable("reading_item_documents");
        entity.HasKey(item => item.ReadingItemId);

        entity.Property(item => item.ReadingItemId).HasColumnName("reading_item_id");
        entity.Property(item => item.SchemaVersion).HasColumnName("schema_version").IsRequired();
        entity.Property(item => item.ContentHash).HasColumnName("content_hash").HasMaxLength(128).IsRequired();
        entity.Property(item => item.TotalBlocks).HasColumnName("total_blocks").IsRequired();
        entity.Property(item => item.TotalPages).HasColumnName("total_pages").IsRequired();
        entity.Property(item => item.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
    }
}
