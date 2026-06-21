using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageReader.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    original_language = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, defaultValue: "Unknown"),
                    storage_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_books", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reading_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    original_language = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    storage_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    content_format = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rss_article_candidates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_key = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    source_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    title = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    external_id = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    published_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    summary = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    author = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    saved_reading_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rss_article_candidates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_settings",
                columns: table => new
                {
                    username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    learning_language = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ai_service_mode = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false, defaultValue: "Fake")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settings", x => x.username);
                });

            migrationBuilder.CreateTable(
                name: "article_metadata",
                columns: table => new
                {
                    reading_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    original_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    published_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    author = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    excerpt = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    rss_feed_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    external_id = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_metadata", x => x.reading_item_id);
                    table.ForeignKey(
                        name: "FK_article_metadata_reading_items_reading_item_id",
                        column: x => x.reading_item_id,
                        principalTable: "reading_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reading_progress",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    reading_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    progress_percent = table.Column<double>(type: "double precision", nullable: false),
                    paragraph_index = table.Column<int>(type: "integer", nullable: false),
                    character_offset = table.Column<int>(type: "integer", nullable: false),
                    last_opened_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_progress", x => x.id);
                    table.ForeignKey(
                        name: "FK_reading_progress_reading_items_reading_item_id",
                        column: x => x.reading_item_id,
                        principalTable: "reading_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vocabulary_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    word = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    translation = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    is_visible_in_vocabulary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    source_language = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    target_language = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    reading_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paragraph_index = table.Column<int>(type: "integer", nullable: false),
                    character_offset = table.Column<int>(type: "integer", nullable: false),
                    selection_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vocabulary_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_vocabulary_entries_reading_items_reading_item_id",
                        column: x => x.reading_item_id,
                        principalTable: "reading_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "related_words",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vocabulary_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    word = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_related_words", x => x.id);
                    table.ForeignKey(
                        name: "FK_related_words_vocabulary_entries_vocabulary_entry_id",
                        column: x => x.vocabulary_entry_id,
                        principalTable: "vocabulary_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "translated_ranges",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    reading_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paragraph_index = table.Column<int>(type: "integer", nullable: false),
                    start_offset = table.Column<int>(type: "integer", nullable: false),
                    end_offset = table.Column<int>(type: "integer", nullable: false),
                    original_text = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    translated_text = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    dictionary_form = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    resolved_selection_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    vocabulary_entry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    show_original = table.Column<bool>(type: "boolean", nullable: false),
                    selection_kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, defaultValue: "Word"),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_translated_ranges", x => x.id);
                    table.ForeignKey(
                        name: "FK_translated_ranges_reading_items_reading_item_id",
                        column: x => x.reading_item_id,
                        principalTable: "reading_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_translated_ranges_vocabulary_entries_vocabulary_entry_id",
                        column: x => x.vocabulary_entry_id,
                        principalTable: "vocabulary_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "vocabulary_examples",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vocabulary_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    translation = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    is_from_book = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    reading_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    paragraph_index = table.Column<int>(type: "integer", nullable: true),
                    character_offset = table.Column<int>(type: "integer", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vocabulary_examples", x => x.id);
                    table.ForeignKey(
                        name: "FK_vocabulary_examples_reading_items_reading_item_id",
                        column: x => x.reading_item_id,
                        principalTable: "reading_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_vocabulary_examples_vocabulary_entries_vocabulary_entry_id",
                        column: x => x.vocabulary_entry_id,
                        principalTable: "vocabulary_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vocabulary_word_details",
                columns: table => new
                {
                    vocabulary_entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    seen_form = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    dictionary_form = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    part_of_speech = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    description = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true),
                    frequency_score = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vocabulary_word_details", x => x.vocabulary_entry_id);
                    table.ForeignKey(
                        name: "FK_vocabulary_word_details_vocabulary_entries_vocabulary_entry~",
                        column: x => x.vocabulary_entry_id,
                        principalTable: "vocabulary_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ai_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    kind = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    provider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    model = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    input_tokens = table.Column<int>(type: "integer", nullable: false),
                    output_tokens = table.Column<int>(type: "integer", nullable: false),
                    total_tokens = table.Column<int>(type: "integer", nullable: false),
                    input_cost_usd = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    output_cost_usd = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    total_cost_usd = table.Column<decimal>(type: "numeric(18,8)", precision: 18, scale: 8, nullable: false),
                    translated_range_id = table.Column<Guid>(type: "uuid", nullable: true),
                    vocabulary_entry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ai_operations", x => x.id);
                    table.ForeignKey(
                        name: "FK_ai_operations_translated_ranges_translated_range_id",
                        column: x => x.translated_range_id,
                        principalTable: "translated_ranges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ai_operations_vocabulary_entries_vocabulary_entry_id",
                        column: x => x.vocabulary_entry_id,
                        principalTable: "vocabulary_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ai_operations_translated_range_id",
                table: "ai_operations",
                column: "translated_range_id");

            migrationBuilder.CreateIndex(
                name: "IX_ai_operations_username",
                table: "ai_operations",
                column: "username");

            migrationBuilder.CreateIndex(
                name: "IX_ai_operations_vocabulary_entry_id",
                table: "ai_operations",
                column: "vocabulary_entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_article_metadata_external_id",
                table: "article_metadata",
                column: "external_id");

            migrationBuilder.CreateIndex(
                name: "IX_books_is_public",
                table: "books",
                column: "is_public");

            migrationBuilder.CreateIndex(
                name: "IX_books_owner_username",
                table: "books",
                column: "owner_username");

            migrationBuilder.CreateIndex(
                name: "IX_reading_items_is_public",
                table: "reading_items",
                column: "is_public");

            migrationBuilder.CreateIndex(
                name: "IX_reading_items_owner_username",
                table: "reading_items",
                column: "owner_username");

            migrationBuilder.CreateIndex(
                name: "IX_reading_items_type",
                table: "reading_items",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_reading_progress_reading_item_id",
                table: "reading_progress",
                column: "reading_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_reading_progress_username_reading_item_id",
                table: "reading_progress",
                columns: new[] { "username", "reading_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_related_words_vocabulary_entry_id",
                table: "related_words",
                column: "vocabulary_entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_rss_article_candidates_source_key",
                table: "rss_article_candidates",
                column: "source_key");

            migrationBuilder.CreateIndex(
                name: "IX_rss_article_candidates_source_key_external_id",
                table: "rss_article_candidates",
                columns: new[] { "source_key", "external_id" });

            migrationBuilder.CreateIndex(
                name: "IX_rss_article_candidates_source_key_url",
                table: "rss_article_candidates",
                columns: new[] { "source_key", "url" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_translated_ranges_reading_item_id_paragraph_index",
                table: "translated_ranges",
                columns: new[] { "reading_item_id", "paragraph_index" });

            migrationBuilder.CreateIndex(
                name: "IX_translated_ranges_username_reading_item_id",
                table: "translated_ranges",
                columns: new[] { "username", "reading_item_id" });

            migrationBuilder.CreateIndex(
                name: "IX_translated_ranges_vocabulary_entry_id",
                table: "translated_ranges",
                column: "vocabulary_entry_id");

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_entries_reading_item_id",
                table: "vocabulary_entries",
                column: "reading_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_entries_username",
                table: "vocabulary_entries",
                column: "username");

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_examples_reading_item_id",
                table: "vocabulary_examples",
                column: "reading_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_vocabulary_examples_vocabulary_entry_id",
                table: "vocabulary_examples",
                column: "vocabulary_entry_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_operations");

            migrationBuilder.DropTable(
                name: "article_metadata");

            migrationBuilder.DropTable(
                name: "books");

            migrationBuilder.DropTable(
                name: "reading_progress");

            migrationBuilder.DropTable(
                name: "related_words");

            migrationBuilder.DropTable(
                name: "rss_article_candidates");

            migrationBuilder.DropTable(
                name: "user_settings");

            migrationBuilder.DropTable(
                name: "vocabulary_examples");

            migrationBuilder.DropTable(
                name: "vocabulary_word_details");

            migrationBuilder.DropTable(
                name: "translated_ranges");

            migrationBuilder.DropTable(
                name: "vocabulary_entries");

            migrationBuilder.DropTable(
                name: "reading_items");
        }
    }
}
