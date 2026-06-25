using LanguageReader.Infrastructure.Features.Settings.Entities;
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
    }
}
