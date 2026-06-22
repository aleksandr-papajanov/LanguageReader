using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.Vocabulary.EntityConfigurations;

internal sealed class VocabularyExampleConfiguration : IEntityTypeConfiguration<VocabularyExampleEntity>
{
    public void Configure(EntityTypeBuilder<VocabularyExampleEntity> entity)
    {
        entity.ToTable("vocabulary_examples");
        entity.HasKey(example => example.Id);

        entity.Property(example => example.Id).HasColumnName("id");
        entity.Property(example => example.VocabularyEntryId).HasColumnName("vocabulary_entry_id");
        entity.Property(example => example.Text).HasColumnName("text").IsRequired();
        entity.Property(example => example.Translation).HasColumnName("translation");
        entity.Property(example => example.IsFromBook).HasColumnName("is_from_book").HasDefaultValue(false);
        entity.Property(example => example.ReadingItemId).HasColumnName("reading_item_id");
        entity.Property(example => example.ParagraphIndex).HasColumnName("paragraph_index");
        entity.Property(example => example.CharacterOffset).HasColumnName("character_offset");
        entity.Property(example => example.CreatedAtUtc).HasColumnName("created_at_utc");

        entity.HasIndex(example => example.VocabularyEntryId);

        entity.HasOne(example => example.VocabularyEntry)
            .WithMany(entry => entry.Examples)
            .HasForeignKey(example => example.VocabularyEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(example => example.ReadingItem)
            .WithMany()
            .HasForeignKey(example => example.ReadingItemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
