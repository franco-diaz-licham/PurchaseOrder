using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PurchaseOrderApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBackgroundOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "background");

            migrationBuilder.CreateTable(
                name: "outbox_message",
                schema: "background",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    message_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    occurred_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    correlation_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false, defaultValue: "pending"),
                    attempt_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    next_attempt_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    locked_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    locked_until_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    processed_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_error = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_message", x => x.id);
                    table.CheckConstraint("ck_outbox_message_attempt_count", "attempt_count >= 0");
                    table.CheckConstraint("ck_outbox_message_correlation_id", "length(trim(correlation_id)) > 0");
                    table.CheckConstraint("ck_outbox_message_entity_type", "length(trim(entity_type)) > 0");
                    table.CheckConstraint("ck_outbox_message_message_type", "length(trim(message_type)) > 0");
                    table.CheckConstraint("ck_outbox_message_status", "status IN ('pending', 'processing', 'processed', 'failed', 'dead_lettered')");
                });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_correlation_id",
                schema: "background",
                table: "outbox_message",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_entity_type_entity_id",
                schema: "background",
                table: "outbox_message",
                columns: new[] { "entity_type", "entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_idempotency_key",
                schema: "background",
                table: "outbox_message",
                column: "idempotency_key",
                unique: true,
                filter: "idempotency_key IS NOT NULL");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_message",
                schema: "background");
        }
    }
}
