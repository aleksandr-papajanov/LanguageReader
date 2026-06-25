using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageReader.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAiExecutionStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "execution_mode",
                table: "ai_operations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "Unknown");

            migrationBuilder.AddColumn<int>(
                name: "turn_count",
                table: "ai_operations",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "tool_call_count",
                table: "ai_operations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "tool_names",
                table: "ai_operations",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "execution_mode",
                table: "ai_operations");

            migrationBuilder.DropColumn(
                name: "turn_count",
                table: "ai_operations");

            migrationBuilder.DropColumn(
                name: "tool_call_count",
                table: "ai_operations");

            migrationBuilder.DropColumn(
                name: "tool_names",
                table: "ai_operations");
        }
    }
}
