using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageReader.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RepairSavedTextKindColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'translated_ranges'
                          AND column_name = 'kind'
                    ) THEN
                        ALTER TABLE translated_ranges ADD COLUMN kind character varying(32);

                        IF EXISTS (
                            SELECT 1
                            FROM information_schema.columns
                            WHERE table_schema = 'public'
                              AND table_name = 'translated_ranges'
                              AND column_name = 'selection_kind'
                        ) THEN
                            UPDATE translated_ranges
                            SET kind = CASE WHEN selection_kind = 'Word' THEN 'LexicalUnit' ELSE 'Text' END;

                            ALTER TABLE translated_ranges DROP COLUMN selection_kind;
                        ELSE
                            UPDATE translated_ranges SET kind = 'LexicalUnit' WHERE kind IS NULL;
                        END IF;

                        ALTER TABLE translated_ranges ALTER COLUMN kind SET NOT NULL;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'translated_ranges'
                          AND column_name = 'resolved_selection_kind'
                    ) THEN
                        ALTER TABLE translated_ranges DROP COLUMN resolved_selection_kind;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'vocabulary_entries'
                          AND column_name = 'kind'
                    ) THEN
                        ALTER TABLE vocabulary_entries ADD COLUMN kind character varying(32);

                        IF EXISTS (
                            SELECT 1
                            FROM information_schema.columns
                            WHERE table_schema = 'public'
                              AND table_name = 'vocabulary_entries'
                              AND column_name = 'selection_kind'
                        ) THEN
                            UPDATE vocabulary_entries
                            SET kind = CASE WHEN selection_kind = 'Word' THEN 'LexicalUnit' ELSE 'Text' END;

                            ALTER TABLE vocabulary_entries DROP COLUMN selection_kind;
                        ELSE
                            UPDATE vocabulary_entries SET kind = 'LexicalUnit' WHERE kind IS NULL;
                        END IF;

                        ALTER TABLE vocabulary_entries ALTER COLUMN kind SET NOT NULL;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'ck_translated_ranges_kind'
                    ) THEN
                        ALTER TABLE translated_ranges
                        ADD CONSTRAINT ck_translated_ranges_kind
                        CHECK (kind IN ('LexicalUnit', 'Text'));
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'ck_vocabulary_entries_kind'
                    ) THEN
                        ALTER TABLE vocabulary_entries
                        ADD CONSTRAINT ck_vocabulary_entries_kind
                        CHECK (kind IN ('LexicalUnit', 'Text'));
                    END IF;
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'ck_translated_ranges_kind'
                    ) THEN
                        ALTER TABLE translated_ranges DROP CONSTRAINT ck_translated_ranges_kind;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'translated_ranges'
                          AND column_name = 'selection_kind'
                    ) THEN
                        ALTER TABLE translated_ranges ADD COLUMN selection_kind character varying(32);
                        UPDATE translated_ranges
                        SET selection_kind = CASE WHEN kind = 'LexicalUnit' THEN 'Word' ELSE 'Text' END;
                        ALTER TABLE translated_ranges ALTER COLUMN selection_kind SET NOT NULL;
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM pg_constraint
                        WHERE conname = 'ck_vocabulary_entries_kind'
                    ) THEN
                        ALTER TABLE vocabulary_entries DROP CONSTRAINT ck_vocabulary_entries_kind;
                    END IF;

                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = 'vocabulary_entries'
                          AND column_name = 'selection_kind'
                    ) THEN
                        ALTER TABLE vocabulary_entries ADD COLUMN selection_kind character varying(32);
                        UPDATE vocabulary_entries
                        SET selection_kind = CASE WHEN kind = 'LexicalUnit' THEN 'Word' ELSE 'Text' END;
                        ALTER TABLE vocabulary_entries ALTER COLUMN selection_kind SET NOT NULL;
                    END IF;
                END $$;
                """);
        }
    }
}
