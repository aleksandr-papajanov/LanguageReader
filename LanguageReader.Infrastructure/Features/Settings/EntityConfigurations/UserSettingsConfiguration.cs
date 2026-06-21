using LanguageReader.Infrastructure.Features.Settings.Entities;
using LanguageReader.Shared.Features.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.Settings.EntityConfigurations;

internal sealed class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettingsEntity>
{
    public void Configure(EntityTypeBuilder<UserSettingsEntity> entity)
    {
        entity.ToTable("user_settings");
        entity.HasKey(settings => settings.Username);

        entity.Property(settings => settings.Username).HasColumnName("username").HasMaxLength(128).IsRequired();
        entity.Property(settings => settings.NativeLanguage).HasColumnName("learning_language").HasMaxLength(64);
        entity.Property(settings => settings.AiServiceMode)
            .HasColumnName("ai_service_mode")
            .HasConversion<string>()
            .HasMaxLength(16)
            .HasDefaultValue(AiServiceMode.Fake)
            .IsRequired();
    }
}
