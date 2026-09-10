using JobAutomationPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobAutomationPlatform.Infrastructure.Persistence.Configurations;

public sealed class ExecutionAttemptConfiguration : IEntityTypeConfiguration<ExecutionAttempt>
{
    public void Configure(EntityTypeBuilder<ExecutionAttempt> builder)
    {
        builder.ToTable("execution_attempts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.ErrorSummary).HasMaxLength(2000);
        builder.Property(x => x.WorkerName).HasMaxLength(200);
        builder.Property(x => x.WorkerInstanceId).HasMaxLength(100);
        builder.Property(x => x.ResponseHeadersJson).HasColumnType("jsonb");
        builder.Property(x => x.ResponseBody).HasColumnType("text");
        builder.Property(x => x.ErrorDetailsJson).HasColumnType("jsonb");
    }
}
