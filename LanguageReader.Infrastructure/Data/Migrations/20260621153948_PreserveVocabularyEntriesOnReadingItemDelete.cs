using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageReader.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class PreserveVocabularyEntriesOnReadingItemDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vocabulary_entries_reading_items_reading_item_id",
                table: "vocabulary_entries");

            migrationBuilder.AlterColumn<Guid>(
                name: "reading_item_id",
                table: "vocabulary_entries",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_vocabulary_entries_reading_items_reading_item_id",
                table: "vocabulary_entries",
                column: "reading_item_id",
                principalTable: "reading_items",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vocabulary_entries_reading_items_reading_item_id",
                table: "vocabulary_entries");

            migrationBuilder.AlterColumn<Guid>(
                name: "reading_item_id",
                table: "vocabulary_entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_vocabulary_entries_reading_items_reading_item_id",
                table: "vocabulary_entries",
                column: "reading_item_id",
                principalTable: "reading_items",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
