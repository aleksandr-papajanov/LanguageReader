using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageReader.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RequireExplicitTranslatedRangeKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "kind",
                table: "translated_ranges",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldDefaultValue: "LexicalUnit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "kind",
                table: "translated_ranges",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "LexicalUnit",
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);
        }
    }
}
