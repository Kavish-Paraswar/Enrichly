using JobAutomationPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobAutomationPlatform.Infrastructure.Persistence.Configurations;

public sealed class ExecutionRequestConfiguration : IEntityTypeConfiguration<ExecutionRequest>
{
    public void Configure(EntityTypeBuilder<ExecutionRequest> builder)
    {
        builder.ToTable("execution_requests");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.OwnerUserId).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.LastErrorSummary).HasMaxLength(2000);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(100);
        builder.Property(x => x.LastErrorDetailsJson).HasColumnType("jsonb");

        builder.HasIndex(x => new { x.OwnerUserId, x.JobId, x.IdempotencyKey })
            .IsUnique()
            .HasFilter("idempotency_key IS NOT NULL");

        builder.HasIndex(x => new { x.JobId, x.CompletedAtUtc })
            .IsUnique()
            .HasFilter("completed_at_utc IS NULL");

        builder.HasIndex(x => new { x.Status, x.ReadyAtUtc });

        builder.HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attempts)
            .WithOne(x => x.ExecutionRequest)
            .HasForeignKey(x => x.ExecutionRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
