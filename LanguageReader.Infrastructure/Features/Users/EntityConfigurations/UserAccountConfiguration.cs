using LanguageReader.Infrastructure.Features.Users.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.Users.EntityConfigurations;

internal sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccountEntity>
{
    public void Configure(EntityTypeBuilder<UserAccountEntity> entity)
    {
        entity.ToTable("user_accounts");
        entity.HasKey(account => account.Username);

        entity.Property(account => account.Username).HasColumnName("username").HasMaxLength(128).IsRequired();
        entity.Property(account => account.Email).HasColumnName("email").HasMaxLength(256);
        entity.Property(account => account.PasswordHash).HasColumnName("password_hash").IsRequired();
        entity.Property(account => account.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();

        entity.HasIndex(account => account.Email).IsUnique();
    }
}
