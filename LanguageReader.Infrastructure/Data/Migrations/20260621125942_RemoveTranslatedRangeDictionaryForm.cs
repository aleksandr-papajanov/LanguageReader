using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageReader.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTranslatedRangeDictionaryForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dictionary_form",
                table: "translated_ranges");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "dictionary_form",
                table: "translated_ranges",
                type: "text",
                nullable: true);
        }
    }
}
