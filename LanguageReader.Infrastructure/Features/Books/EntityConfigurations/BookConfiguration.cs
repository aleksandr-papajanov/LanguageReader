using LanguageReader.Infrastructure.Features.Books.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LanguageReader.Infrastructure.Features.Books.EntityConfigurations;

internal sealed class BookConfiguration : IEntityTypeConfiguration<BookEntity>
{
    public void Configure(EntityTypeBuilder<BookEntity> entity)
    {
        entity.ToTable("books");

        entity.HasKey(book => book.Id);

        entity.Property(book => book.Id)
            .HasColumnName("id");

        entity.Property(book => book.OwnerUsername)
            .HasColumnName("owner_username")
            .HasMaxLength(128)
            .IsRequired();

        entity.Property(book => book.Title)
            .HasColumnName("title")
            .HasMaxLength(512)
            .IsRequired();

        entity.Property(book => book.OriginalFileName)
            .HasColumnName("original_file_name")
            .HasMaxLength(512)
            .IsRequired();

        entity.Property(book => book.OriginalLanguage)
            .HasColumnName("original_language")
            .HasMaxLength(64)
            .HasDefaultValue("Unknown")
            .IsRequired();

        entity.Property(book => book.StoragePath)
            .HasColumnName("storage_path")
            .HasMaxLength(1024)
            .IsRequired();

        entity.Property(book => book.IsPublic)
            .HasColumnName("is_public");

        entity.Property(book => book.CreatedAtUtc)
            .HasColumnName("created_at_utc");

        entity.HasIndex(book => book.OwnerUsername);
        entity.HasIndex(book => book.IsPublic);

    }
}

