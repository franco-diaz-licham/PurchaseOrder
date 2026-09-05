using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseOrderApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyOutboxPublishing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Preserve completed legacy messages so they are never published again.
            migrationBuilder.Sql("""UPDATE background.outbox_message SET processed_utc = COALESCE(processed_utc, updated_utc) WHERE status = 'processed';""");

            migrationBuilder.DropIndex(
                name: "ix_outbox_message_locked_until_utc",
                schema: "background",
                table: "outbox_message");

            migrationBuilder.DropIndex(
                name: "ix_outbox_message_status_next_attempt_utc",
                schema: "background",
                table: "outbox_message");

            migrationBuilder.DropCheckConstraint(
                name: "ck_outbox_message_attempt_count",
                schema: "background",
                table: "outbox_message");

            migrationBuilder.DropCheckConstraint(
                name: "ck_outbox_message_status",
                schema: "background",
                table: "outbox_message");

            migrationBuilder.DropColumn(
                name: "attempt_count",
                schema: "background",
                table: "outbox_message");

            migrationBuilder.DropColumn(
                name: "last_error",
                schema: "background",
                table: "outbox_message");

            migrationBuilder.DropColumn(
                name: "locked_by",
                schema: "background",
                table: "outbox_message");

            migrationBuilder.DropColumn(
                name: "locked_until_utc",
                schema: "background",
                table: "outbox_message");

            migrationBuilder.DropColumn(
                name: "next_attempt_utc",
                schema: "background",
                table: "outbox_message");

            migrationBuilder.DropColumn(
                name: "status",
                schema: "background",
                table: "outbox_message");

            migrationBuilder.DropColumn(
                name: "updated_utc",
                schema: "background",
                table: "outbox_message");

            migrationBuilder.RenameColumn(
                name: "processed_utc",
                schema: "background",
                table: "outbox_message",
                newName: "published_utc");

            migrationBuilder.AddColumn<string>(
                name: "hangfire_job_id",
                schema: "background",
                table: "outbox_message",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_created_utc",
                schema: "background",
                table: "outbox_message",
                column: "created_utc",
                filter: "published_utc IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outbox_message_created_utc",
                schema: "background",
                table: "outbox_message");

            migrationBuilder.DropColumn(
                name: "hangfire_job_id",
                schema: "background",
                table: "outbox_message");

            migrationBuilder.RenameColumn(
                name: "published_utc",
                schema: "background",
                table: "outbox_message",
                newName: "processed_utc");

            migrationBuilder.AddColumn<int>(
                name: "attempt_count",
                schema: "background",
                table: "outbox_message",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "last_error",
                schema: "background",
                table: "outbox_message",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "locked_by",
                schema: "background",
                table: "outbox_message",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "locked_until_utc",
                schema: "background",
                table: "outbox_message",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "next_attempt_utc",
                schema: "background",
                table: "outbox_message",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "status",
                schema: "background",
                table: "outbox_message",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "pending");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated_utc",
                schema: "background",
                table: "outbox_message",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()");

            migrationBuilder.Sql("UPDATE background.outbox_message SET status = 'processed' WHERE processed_utc IS NOT NULL;");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_locked_until_utc",
                schema: "background",
                table: "outbox_message",
                column: "locked_until_utc");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_status_next_attempt_utc",
                schema: "background",
                table: "outbox_message",
                columns: new[] { "status", "next_attempt_utc" });

            migrationBuilder.AddCheckConstraint(
                name: "ck_outbox_message_attempt_count",
                schema: "background",
                table: "outbox_message",
                sql: "attempt_count >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "ck_outbox_message_status",
                schema: "background",
                table: "outbox_message",
                sql: "status IN ('pending', 'processing', 'processed', 'failed', 'dead_lettered')");
        }
    }
}
