using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageReader.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class StoreReadingItemCanonicalContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "reading_item_assets",
                columns: table => new
                {
                    reading_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    asset_id = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    content_type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    storage_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    alt_text = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    is_cover = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_item_assets", x => new { x.reading_item_id, x.asset_id });
                    table.ForeignKey(
                        name: "FK_reading_item_assets_reading_items_reading_item_id",
                        column: x => x.reading_item_id,
                        principalTable: "reading_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reading_item_blocks",
                columns: table => new
                {
                    reading_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence_index = table.Column<int>(type: "integer", nullable: false),
                    block_index = table.Column<int>(type: "integer", nullable: true),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    image_id = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    weight = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_item_blocks", x => new { x.reading_item_id, x.sequence_index });
                    table.ForeignKey(
                        name: "FK_reading_item_blocks_reading_items_reading_item_id",
                        column: x => x.reading_item_id,
                        principalTable: "reading_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reading_item_documents",
                columns: table => new
                {
                    reading_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    schema_version = table.Column<int>(type: "integer", nullable: false),
                    content_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    total_blocks = table.Column<int>(type: "integer", nullable: false),
                    total_pages = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_item_documents", x => x.reading_item_id);
                    table.ForeignKey(
                        name: "FK_reading_item_documents_reading_items_reading_item_id",
                        column: x => x.reading_item_id,
                        principalTable: "reading_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reading_item_pages",
                columns: table => new
                {
                    reading_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    page_index = table.Column<int>(type: "integer", nullable: false),
                    start_sequence_index = table.Column<int>(type: "integer", nullable: false),
                    end_sequence_index = table.Column<int>(type: "integer", nullable: false),
                    start_block_index = table.Column<int>(type: "integer", nullable: false),
                    end_block_index = table.Column<int>(type: "integer", nullable: false),
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
                name: "IX_reading_item_assets_reading_item_id_is_cover",
                table: "reading_item_assets",
                columns: new[] { "reading_item_id", "is_cover" });

            migrationBuilder.CreateIndex(
                name: "IX_reading_item_blocks_reading_item_id_block_index",
                table: "reading_item_blocks",
                columns: new[] { "reading_item_id", "block_index" });

            migrationBuilder.CreateIndex(
                name: "IX_reading_item_pages_reading_item_id_start_block_index_end_bl~",
                table: "reading_item_pages",
                columns: new[] { "reading_item_id", "start_block_index", "end_block_index" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reading_item_assets");

            migrationBuilder.DropTable(
                name: "reading_item_blocks");

            migrationBuilder.DropTable(
                name: "reading_item_documents");

            migrationBuilder.DropTable(
                name: "reading_item_pages");
        }
    }
}
