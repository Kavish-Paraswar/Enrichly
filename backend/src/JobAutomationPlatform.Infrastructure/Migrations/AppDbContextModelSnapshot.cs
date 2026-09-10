using System;
using JobAutomationPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace JobAutomationPlatform.Infrastructure.Migrations;

[DbContext(typeof(AppDbContext))]
public partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

        modelBuilder.Entity("JobAutomationPlatform.Domain.Entities.ExecutionAttempt", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid")
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

                b.Property<Guid>("ExecutionRequestId")
                    .HasColumnType("uuid")
                    .HasColumnName("execution_request_id");

                b.Property<int>("AttemptNumber")
                    .HasColumnType("integer")
                    .HasColumnName("attempt_number");

                b.Property<DateTimeOffset?>("CompletedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("completed_at_utc");

                b.Property<DateTimeOffset>("CreatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("created_at_utc");

                b.Property<string>("ErrorDetailsJson")
                    .HasColumnType("text")
                    .HasColumnName("error_details_json");

                b.Property<string>("ErrorSummary")
                    .HasMaxLength(2000)
                    .HasColumnType("character varying(2000)")
                    .HasColumnName("error_summary");

                b.Property<DateTimeOffset>("HeartbeatAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("heartbeat_at_utc");

                b.Property<DateTimeOffset>("StartedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("started_at_utc");

                b.Property<int>("Status")
                    .HasColumnType("integer")
                    .HasColumnName("status");

                b.Property<DateTimeOffset>("UpdatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("updated_at_utc");

                b.Property<string>("WorkerName")
                    .HasMaxLength(200)
                    .HasColumnType("character varying(200)")
                    .HasColumnName("worker_name");

                b.HasKey("Id");

                b.HasIndex("ExecutionRequestId");

                b.ToTable("execution_attempts");
            });

        modelBuilder.Entity("JobAutomationPlatform.Domain.Entities.ExecutionRequest", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid")
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

                b.Property<Guid>("JobId")
                    .HasColumnType("uuid")
                    .HasColumnName("job_id");

                b.Property<DateTimeOffset?>("CompletedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("completed_at_utc");

                b.Property<DateTimeOffset>("CreatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("created_at_utc");

                b.Property<string>("LastErrorSummary")
                    .HasMaxLength(2000)
                    .HasColumnType("character varying(2000)")
                    .HasColumnName("last_error_summary");

                b.Property<DateTimeOffset>("ReadyAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("ready_at_utc");

                b.Property<DateTimeOffset>("RequestedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("requested_at_utc");

                b.Property<int>("RetryCount")
                    .HasColumnType("integer")
                    .HasColumnName("retry_count");

                b.Property<DateTimeOffset?>("StartedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("started_at_utc");

                b.Property<int>("Source")
                    .HasColumnType("integer")
                    .HasColumnName("source");

                b.Property<int>("Status")
                    .HasColumnType("integer")
                    .HasColumnName("status");

                b.Property<DateTimeOffset>("UpdatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("updated_at_utc");

                b.HasKey("Id");

                b.HasIndex("JobId", "CompletedAtUtc")
                    .IsUnique()
                    .HasFilter("completed_at_utc IS NULL");

                b.HasIndex("Status", "ReadyAtUtc");

                b.ToTable("execution_requests");
            });

        modelBuilder.Entity("JobAutomationPlatform.Domain.Entities.Job", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid")
                    .HasColumnName("id")
                    .HasDefaultValueSql("gen_random_uuid()");

                b.Property<bool>("IsEnabled")
                    .HasColumnType("boolean")
                    .HasColumnName("is_enabled");

                b.Property<string>("Description")
                    .HasMaxLength(2000)
                    .HasColumnType("character varying(2000)")
                    .HasColumnName("description");

                b.Property<string>("HttpMethod")
                    .IsRequired()
                    .HasMaxLength(16)
                    .HasColumnType("character varying(16)")
                    .HasColumnName("http_method");

                b.Property<int>("MaxAttempts")
                    .HasColumnType("integer")
                    .HasColumnName("max_attempts");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("character varying(200)")
                    .HasColumnName("name");

                b.Property<DateTimeOffset?>("NextRunAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("next_run_at_utc");

                b.Property<string>("PayloadJson")
                    .HasColumnType("jsonb")
                    .HasColumnName("payload_json");

                b.Property<int?>("ScheduleEveryMinutes")
                    .HasColumnType("integer")
                    .HasColumnName("schedule_every_minutes");

                b.Property<string>("TargetUrl")
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnType("character varying(500)")
                    .HasColumnName("target_url");

                b.Property<DateTimeOffset>("CreatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("created_at_utc");

                b.Property<DateTimeOffset>("UpdatedAtUtc")
                    .HasColumnType("timestamp with time zone")
                    .HasColumnName("updated_at_utc");

                b.Property<long>("Version")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("bigint")
                    .HasColumnName("version")
                    .HasDefaultValue(1L);

                b.HasKey("Id");

                b.ToTable("jobs");
            });

        modelBuilder.Entity("JobAutomationPlatform.Domain.Entities.ExecutionAttempt", b =>
            {
                b.HasOne("JobAutomationPlatform.Domain.Entities.ExecutionRequest", "ExecutionRequest")
                    .WithMany("Attempts")
                    .HasForeignKey("ExecutionRequestId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("ExecutionRequest");
            });

        modelBuilder.Entity("JobAutomationPlatform.Domain.Entities.ExecutionRequest", b =>
            {
                b.HasOne("JobAutomationPlatform.Domain.Entities.Job", "Job")
                    .WithMany("ExecutionRequests")
                    .HasForeignKey("JobId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Job");

                b.Navigation("Attempts");
            });

        modelBuilder.Entity("JobAutomationPlatform.Domain.Entities.Job", b =>
            {
                b.Navigation("ExecutionRequests");
            });
    }
}
