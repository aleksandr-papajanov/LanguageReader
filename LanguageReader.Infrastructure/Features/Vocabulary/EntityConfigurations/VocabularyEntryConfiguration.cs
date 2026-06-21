using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.Vocabulary.EntityConfigurations;

internal sealed class VocabularyEntryConfiguration : IEntityTypeConfiguration<VocabularyEntryEntity>
{
    public void Configure(EntityTypeBuilder<VocabularyEntryEntity> entity)
    {
        entity.ToTable("vocabulary_entries");
        entity.HasKey(entry => entry.Id);

        entity.Property(entry => entry.Id).HasColumnName("id");
        entity.Property(entry => entry.Username).HasColumnName("username").HasMaxLength(128).IsRequired();
        entity.Property(entry => entry.Word).HasColumnName("word").IsRequired();
        entity.Property(entry => entry.Translation).HasColumnName("translation").IsRequired();
        entity.Property(entry => entry.IsVisibleInVocabulary).HasColumnName("is_visible_in_vocabulary").HasDefaultValue(true);
        entity.Property(entry => entry.SourceLanguage).HasColumnName("source_language").HasMaxLength(64);
        entity.Property(entry => entry.TargetLanguage).HasColumnName("target_language").HasMaxLength(64).IsRequired();
        entity.Property(entry => entry.ReadingItemId).HasColumnName("reading_item_id");
        entity.Property(entry => entry.ParagraphIndex).HasColumnName("paragraph_index");
        entity.Property(entry => entry.CharacterOffset).HasColumnName("character_offset");
        entity.Property(entry => entry.SelectionKind).HasColumnName("selection_kind").HasConversion<string>().HasMaxLength(32);
        entity.Property(entry => entry.CreatedAtUtc).HasColumnName("created_at_utc");

        entity.HasIndex(entry => entry.Username);
        entity.HasIndex(entry => entry.ReadingItemId);

        entity.HasOne(entry => entry.Book)
            .WithMany(item => item.VocabularyEntries)
            .HasForeignKey(entry => entry.ReadingItemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

