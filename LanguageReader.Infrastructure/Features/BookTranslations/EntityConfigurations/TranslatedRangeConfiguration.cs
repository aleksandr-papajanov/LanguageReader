using LanguageReader.Infrastructure.Features.BookTranslations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.BookTranslations.EntityConfigurations;

internal sealed class TranslatedRangeConfiguration : IEntityTypeConfiguration<TranslatedRangeEntity>
{
    public void Configure(EntityTypeBuilder<TranslatedRangeEntity> entity)
    {
        entity.ToTable("translated_ranges");
        entity.HasKey(range => range.Id);

        entity.Property(range => range.Id).HasColumnName("id");
        entity.Property(range => range.Username).HasColumnName("username").HasMaxLength(128).IsRequired();
        entity.Property(range => range.ReadingItemId).HasColumnName("reading_item_id");
        entity.Property(range => range.ParagraphIndex).HasColumnName("paragraph_index");
        entity.Property(range => range.StartOffset).HasColumnName("start_offset");
        entity.Property(range => range.EndOffset).HasColumnName("end_offset");
        entity.Property(range => range.OriginalText).HasColumnName("original_text").IsRequired();
        entity.Property(range => range.TranslatedText).HasColumnName("translated_text").IsRequired();
        entity.Property(range => range.DictionaryForm).HasColumnName("dictionary_form");
        entity.Property(range => range.ResolvedSelectionKind).HasColumnName("resolved_selection_kind").HasConversion<string>().HasMaxLength(32);
        entity.Property(range => range.VocabularyEntryId).HasColumnName("vocabulary_entry_id");
        entity.Property(range => range.ShowOriginal).HasColumnName("show_original");
        entity.Property(range => range.SelectionKind).HasColumnName("selection_kind").HasConversion<string>().HasMaxLength(32).HasDefaultValue(SelectionKind.Word);
        entity.Property(range => range.CreatedAtUtc).HasColumnName("created_at_utc");

        entity.HasIndex(range => new { range.Username, range.ReadingItemId });
        entity.HasIndex(range => new { range.ReadingItemId, range.ParagraphIndex });

        entity.HasOne(range => range.Book)
            .WithMany(item => item.TranslatedRanges)
            .HasForeignKey(range => range.ReadingItemId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasIndex(range => range.VocabularyEntryId);

        entity.HasOne(range => range.VocabularyEntry)
            .WithMany()
            .HasForeignKey(range => range.VocabularyEntryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

