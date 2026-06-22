using LanguageReader.Infrastructure.Features.Reading.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.Reading.EntityConfigurations;

internal sealed class ReadingProgressConfiguration : IEntityTypeConfiguration<ReadingProgressEntity>
{
    public void Configure(EntityTypeBuilder<ReadingProgressEntity> entity)
    {
        entity.ToTable("reading_progress");
        entity.HasKey(progress => progress.Id);

        entity.Property(progress => progress.Id).HasColumnName("id");
        entity.Property(progress => progress.Username).HasColumnName("username").HasMaxLength(128).IsRequired();
        entity.Property(progress => progress.ReadingItemId).HasColumnName("reading_item_id");
        entity.Property(progress => progress.ProgressPercent).HasColumnName("progress_percent");
        entity.Property(progress => progress.ParagraphIndex).HasColumnName("paragraph_index");
        entity.Property(progress => progress.CharacterOffset).HasColumnName("character_offset");
        entity.Property(progress => progress.LastOpenedAtUtc).HasColumnName("last_opened_at_utc");

        entity.HasIndex(progress => new { progress.Username, progress.ReadingItemId }).IsUnique();

        entity.HasOne(progress => progress.ReadingItem)
            .WithMany(item => item.ReadingProgresses)
            .HasForeignKey(progress => progress.ReadingItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

