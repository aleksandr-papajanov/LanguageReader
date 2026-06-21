using LanguageReader.Infrastructure.Features.Vocabulary.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.Vocabulary.EntityConfigurations;

internal sealed class VocabularyWordDetailsConfiguration : IEntityTypeConfiguration<VocabularyWordDetailsEntity>
{
    public void Configure(EntityTypeBuilder<VocabularyWordDetailsEntity> entity)
    {
        entity.ToTable("vocabulary_word_details");
        entity.HasKey(details => details.VocabularyEntryId);

        entity.Property(details => details.VocabularyEntryId).HasColumnName("vocabulary_entry_id");
        entity.Property(details => details.SeenForm).HasColumnName("seen_form");
        entity.Property(details => details.DictionaryForm).HasColumnName("dictionary_form");
        entity.Property(details => details.PartOfSpeech).HasColumnName("part_of_speech").HasMaxLength(64);
        entity.Property(details => details.Description).HasColumnName("description");
        entity.Property(details => details.FrequencyScore).HasColumnName("frequency_score");
        entity.Property(details => details.Notes).HasColumnName("notes");

        entity.HasOne(details => details.VocabularyEntry)
            .WithOne(entry => entry.WordDetails)
            .HasForeignKey<VocabularyWordDetailsEntity>(details => details.VocabularyEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
