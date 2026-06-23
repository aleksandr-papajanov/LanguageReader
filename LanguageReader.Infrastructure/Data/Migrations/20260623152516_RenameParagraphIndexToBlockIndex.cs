using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageReader.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameParagraphIndexToBlockIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "paragraph_index",
                table: "vocabulary_examples",
                newName: "block_index");

            migrationBuilder.RenameColumn(
                name: "paragraph_index",
                table: "vocabulary_entries",
                newName: "block_index");

            migrationBuilder.RenameColumn(
                name: "paragraph_index",
                table: "translated_ranges",
                newName: "block_index");

            migrationBuilder.RenameIndex(
                name: "IX_translated_ranges_reading_item_id_paragraph_index",
                table: "translated_ranges",
                newName: "IX_translated_ranges_reading_item_id_block_index");

            migrationBuilder.RenameColumn(
                name: "paragraph_index",
                table: "reading_progress",
                newName: "block_index");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "block_index",
                table: "vocabulary_examples",
                newName: "paragraph_index");

            migrationBuilder.RenameColumn(
                name: "block_index",
                table: "vocabulary_entries",
                newName: "paragraph_index");

            migrationBuilder.RenameColumn(
                name: "block_index",
                table: "translated_ranges",
                newName: "paragraph_index");

            migrationBuilder.RenameIndex(
                name: "IX_translated_ranges_reading_item_id_block_index",
                table: "translated_ranges",
                newName: "IX_translated_ranges_reading_item_id_paragraph_index");

            migrationBuilder.RenameColumn(
                name: "block_index",
                table: "reading_progress",
                newName: "paragraph_index");
        }
    }
}
