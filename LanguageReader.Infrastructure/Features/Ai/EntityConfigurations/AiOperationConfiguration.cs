using LanguageReader.Infrastructure.Features.Ai.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.Ai.EntityConfigurations;

internal sealed class AiOperationConfiguration : IEntityTypeConfiguration<AiOperationEntity>
{
    public void Configure(EntityTypeBuilder<AiOperationEntity> entity)
    {
        entity.ToTable("ai_operations");
        entity.HasKey(operation => operation.Id);

        entity.Property(operation => operation.Id).HasColumnName("id");
        entity.Property(operation => operation.Username).HasColumnName("username").HasMaxLength(128).IsRequired();
        entity.Property(operation => operation.Kind).HasColumnName("kind").HasConversion<string>().HasMaxLength(64).IsRequired();
        entity.Property(operation => operation.Provider).HasColumnName("provider").HasMaxLength(128).IsRequired();
        entity.Property(operation => operation.Model).HasColumnName("model").HasMaxLength(256).IsRequired();
        entity.Property(operation => operation.InputTokens).HasColumnName("input_tokens");
        entity.Property(operation => operation.OutputTokens).HasColumnName("output_tokens");
        entity.Property(operation => operation.TotalTokens).HasColumnName("total_tokens");
        entity.Property(operation => operation.InputCostUsd).HasColumnName("input_cost_usd").HasPrecision(18, 8);
        entity.Property(operation => operation.OutputCostUsd).HasColumnName("output_cost_usd").HasPrecision(18, 8);
        entity.Property(operation => operation.TotalCostUsd).HasColumnName("total_cost_usd").HasPrecision(18, 8);
        entity.Property(operation => operation.TranslatedRangeId).HasColumnName("translated_range_id");
        entity.Property(operation => operation.VocabularyEntryId).HasColumnName("vocabulary_entry_id");
        entity.Property(operation => operation.CreatedAtUtc).HasColumnName("created_at_utc");

        entity.HasIndex(operation => operation.Username);
        entity.HasIndex(operation => operation.TranslatedRangeId);
        entity.HasIndex(operation => operation.VocabularyEntryId);

        entity.HasOne(operation => operation.TranslatedRange)
            .WithMany(range => range.AiOperations)
            .HasForeignKey(operation => operation.TranslatedRangeId)
            .OnDelete(DeleteBehavior.SetNull);

        entity.HasOne(operation => operation.VocabularyEntry)
            .WithMany(entry => entry.AiOperations)
            .HasForeignKey(operation => operation.VocabularyEntryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
