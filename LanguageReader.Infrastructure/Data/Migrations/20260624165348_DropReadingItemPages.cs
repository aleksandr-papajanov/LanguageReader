using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageReader.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropReadingItemPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reading_item_pages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reading_item_pages",
                columns: table => new
                {
                    reading_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    page_index = table.Column<int>(type: "integer", nullable: false),
                    end_block_index = table.Column<int>(type: "integer", nullable: false),
                    end_sequence_index = table.Column<int>(type: "integer", nullable: false),
                    start_block_index = table.Column<int>(type: "integer", nullable: false),
                    start_sequence_index = table.Column<int>(type: "integer", nullable: false),
                    weight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_item_pages", x => new { x.reading_item_id, x.page_index });
                    table.ForeignKey(
                        name: "FK_reading_item_pages_reading_items_reading_item_id",
                        column: x => x.reading_item_id,
                        principalTable: "reading_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reading_item_pages_reading_item_id_start_block_index_end_bl~",
                table: "reading_item_pages",
                columns: new[] { "reading_item_id", "start_block_index", "end_block_index" });
        }
    }
}
