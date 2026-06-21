using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.Vocabulary.EntityConfigurations;

internal sealed class RelatedWordConfiguration : IEntityTypeConfiguration<RelatedWordEntity>
{
    public void Configure(EntityTypeBuilder<RelatedWordEntity> entity)
    {
        entity.ToTable("related_words");
        entity.HasKey(word => word.Id);

        entity.Property(word => word.Id).HasColumnName("id");
        entity.Property(word => word.VocabularyEntryId).HasColumnName("vocabulary_entry_id");
        entity.Property(word => word.Word).HasColumnName("word").IsRequired();
        entity.Property(word => word.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(32).IsRequired();
        entity.Property(word => word.SortOrder).HasColumnName("sort_order");
        entity.Property(word => word.CreatedAtUtc).HasColumnName("created_at_utc");

        entity.HasIndex(word => word.VocabularyEntryId);

        entity.HasOne(word => word.VocabularyEntry)
            .WithMany(entry => entry.RelatedWords)
            .HasForeignKey(word => word.VocabularyEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
