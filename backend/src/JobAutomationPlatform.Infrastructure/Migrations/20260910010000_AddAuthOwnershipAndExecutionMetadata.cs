using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAutomationPlatform.Infrastructure.Migrations;

public partial class AddAuthOwnershipAndExecutionMetadata : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_users", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "session_tokens",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                token_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                token_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                expires_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                revoked_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                revoked_reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                ip_address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                user_agent = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_session_tokens", x => x.id);
                table.ForeignKey(
                    name: "fk_session_tokens_users_user_id",
                    column: x => x.user_id,
                    principalTable: "users",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_users_email",
            table: "users",
            column: "email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_session_tokens_token_id",
            table: "session_tokens",
            column: "token_id",
            unique: true);

        migrationBuilder.AddColumn<Guid>(
            name: "owner_user_id",
            table: "jobs",
            type: "uuid",
            nullable: false);

        migrationBuilder.AddColumn<string>(
            name: "request_headers_json",
            table: "jobs",
            type: "jsonb",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "timeout_seconds",
            table: "jobs",
            type: "integer",
            nullable: false,
            defaultValue: 30);

        migrationBuilder.AddColumn<Guid>(
            name: "owner_user_id",
            table: "execution_requests",
            type: "uuid",
            nullable: false);

        migrationBuilder.AddColumn<string>(
            name: "idempotency_key",
            table: "execution_requests",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "retry_at_utc",
            table: "execution_requests",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "last_error_details_json",
            table: "execution_requests",
            type: "jsonb",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "response_headers_json",
            table: "execution_attempts",
            type: "jsonb",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "response_body",
            table: "execution_attempts",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "worker_instance_id",
            table: "execution_attempts",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "http_status_code",
            table: "execution_attempts",
            type: "integer",
            nullable: true);

        migrationBuilder.AddColumn<long>(
            name: "duration_milliseconds",
            table: "execution_attempts",
            type: "bigint",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "is_timed_out",
            table: "execution_attempts",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddForeignKey(
            name: "fk_jobs_users_owner_user_id",
            table: "jobs",
            column: "owner_user_id",
            principalTable: "users",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "fk_execution_requests_users_owner_user_id",
            table: "execution_requests",
            column: "owner_user_id",
            principalTable: "users",
            principalColumn: "id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.CreateIndex(
            name: "ix_jobs_owner_user_id_name",
            table: "jobs",
            columns: new[] { "owner_user_id", "name" });

        migrationBuilder.CreateIndex(
            name: "ix_execution_requests_owner_user_id_job_id_idempotency_key",
            table: "execution_requests",
            columns: new[] { "owner_user_id", "job_id", "idempotency_key" },
            unique: true,
            filter: "idempotency_key IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "fk_jobs_users_owner_user_id", table: "jobs");
        migrationBuilder.DropForeignKey(name: "fk_execution_requests_users_owner_user_id", table: "execution_requests");

        migrationBuilder.DropTable(name: "session_tokens");
        migrationBuilder.DropTable(name: "users");

        migrationBuilder.DropIndex(name: "ix_jobs_owner_user_id_name", table: "jobs");
        migrationBuilder.DropIndex(name: "ix_execution_requests_owner_user_id_job_id_idempotency_key", table: "execution_requests");

        migrationBuilder.DropColumn(name: "owner_user_id", table: "jobs");
        migrationBuilder.DropColumn(name: "request_headers_json", table: "jobs");
        migrationBuilder.DropColumn(name: "timeout_seconds", table: "jobs");

        migrationBuilder.DropColumn(name: "owner_user_id", table: "execution_requests");
        migrationBuilder.DropColumn(name: "idempotency_key", table: "execution_requests");
        migrationBuilder.DropColumn(name: "retry_at_utc", table: "execution_requests");
        migrationBuilder.DropColumn(name: "last_error_details_json", table: "execution_requests");

        migrationBuilder.DropColumn(name: "response_headers_json", table: "execution_attempts");
        migrationBuilder.DropColumn(name: "response_body", table: "execution_attempts");
        migrationBuilder.DropColumn(name: "worker_instance_id", table: "execution_attempts");
        migrationBuilder.DropColumn(name: "http_status_code", table: "execution_attempts");
        migrationBuilder.DropColumn(name: "duration_milliseconds", table: "execution_attempts");
        migrationBuilder.DropColumn(name: "is_timed_out", table: "execution_attempts");
    }
}
