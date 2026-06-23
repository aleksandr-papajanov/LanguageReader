using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageReader.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameVocabularyExampleReadingItemFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_from_book",
                table: "vocabulary_examples",
                newName: "is_from_reading_item");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "is_from_reading_item",
                table: "vocabulary_examples",
                newName: "is_from_book");
        }
    }
}
