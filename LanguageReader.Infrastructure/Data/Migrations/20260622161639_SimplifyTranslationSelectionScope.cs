using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageReader.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyTranslationSelectionScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "selection_kind",
                table: "translated_ranges",
                newName: "kind");

            migrationBuilder.RenameColumn(
                name: "selection_kind",
                table: "vocabulary_entries",
                newName: "kind");

            migrationBuilder.Sql(
                """
                UPDATE translated_ranges
                SET kind = CASE WHEN kind = 'Word' THEN 'LexicalUnit' ELSE 'Text' END;
                """);

            migrationBuilder.Sql(
                """
                UPDATE vocabulary_entries
                SET kind = CASE WHEN kind = 'Word' THEN 'LexicalUnit' ELSE 'Text' END;
                """);

            migrationBuilder.DropColumn(
                name: "resolved_selection_kind",
                table: "translated_ranges");

            migrationBuilder.AddCheckConstraint(
                name: "ck_translated_ranges_kind",
                table: "translated_ranges",
                sql: "kind IN ('LexicalUnit', 'Text')");

            migrationBuilder.AddCheckConstraint(
                name: "ck_vocabulary_entries_kind",
                table: "vocabulary_entries",
                sql: "kind IN ('LexicalUnit', 'Text')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_translated_ranges_kind",
                table: "translated_ranges");

            migrationBuilder.DropCheckConstraint(
                name: "ck_vocabulary_entries_kind",
                table: "vocabulary_entries");

            migrationBuilder.Sql(
                """
                UPDATE translated_ranges
                SET kind = CASE WHEN kind = 'LexicalUnit' THEN 'Word' ELSE 'Text' END;
                """);

            migrationBuilder.Sql(
                """
                UPDATE vocabulary_entries
                SET kind = CASE WHEN kind = 'LexicalUnit' THEN 'Word' ELSE 'Text' END;
                """);

            migrationBuilder.RenameColumn(
                name: "kind",
                table: "translated_ranges",
                newName: "selection_kind");

            migrationBuilder.RenameColumn(
                name: "kind",
                table: "vocabulary_entries",
                newName: "selection_kind");

            migrationBuilder.AddColumn<string>(
                name: "resolved_selection_kind",
                table: "translated_ranges",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);
        }
    }
}
