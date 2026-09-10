using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAutomationPlatform.Infrastructure.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pgcrypto;");

        migrationBuilder.CreateTable(
            name: "jobs",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                target_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                http_method = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                payload_json = table.Column<string>(type: "jsonb", nullable: true),
                is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                schedule_every_minutes = table.Column<int>(type: "integer", nullable: true),
                next_run_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                max_attempts = table.Column<int>(type: "integer", nullable: false),
                version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 1L),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_jobs", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "execution_requests",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                job_id = table.Column<Guid>(type: "uuid", nullable: false),
                source = table.Column<int>(type: "integer", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                retry_count = table.Column<int>(type: "integer", nullable: false),
                requested_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ready_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                last_error_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_execution_requests", x => x.id);
                table.ForeignKey(
                    name: "fk_execution_requests_jobs_job_id",
                    column: x => x.job_id,
                    principalTable: "jobs",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "execution_attempts",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                execution_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                attempt_number = table.Column<int>(type: "integer", nullable: false),
                status = table.Column<int>(type: "integer", nullable: false),
                worker_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                heartbeat_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                error_summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                error_details_json = table.Column<string>(type: "text", nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_execution_attempts", x => x.id);
                table.ForeignKey(
                    name: "fk_execution_attempts_execution_requests_execution_request_id",
                    column: x => x.execution_request_id,
                    principalTable: "execution_requests",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_execution_attempts_execution_request_id",
            table: "execution_attempts",
            column: "execution_request_id");

        migrationBuilder.CreateIndex(
            name: "ix_execution_requests_job_id_completed_at_utc",
            table: "execution_requests",
            columns: new[] { "job_id", "completed_at_utc" },
            unique: true,
            filter: "completed_at_utc IS NULL");

        migrationBuilder.CreateIndex(
            name: "ix_execution_requests_status_ready_at_utc",
            table: "execution_requests",
            columns: new[] { "status", "ready_at_utc" });

        migrationBuilder.CreateIndex(
            name: "ix_execution_requests_job_id",
            table: "execution_requests",
            column: "job_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "execution_attempts");
        migrationBuilder.DropTable(name: "execution_requests");
        migrationBuilder.DropTable(name: "jobs");
    }
}
