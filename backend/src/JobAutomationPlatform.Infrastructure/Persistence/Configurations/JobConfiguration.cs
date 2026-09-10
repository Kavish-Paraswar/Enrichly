using JobAutomationPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobAutomationPlatform.Infrastructure.Persistence.Configurations;

public sealed class JobConfiguration : IEntityTypeConfiguration<Job>
{
    public void Configure(EntityTypeBuilder<Job> builder)
    {
        builder.ToTable("jobs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.OwnerUserId).IsRequired();
        builder.Property(x => x.Version).HasDefaultValue(1);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Property(x => x.TargetUrl).IsRequired();
        builder.Property(x => x.HttpMethod).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).IsRequired();
        builder.Property(x => x.RequestHeadersJson).HasColumnType("jsonb");
        builder.Property(x => x.PayloadJson).HasColumnType("jsonb");
        builder.Property(x => x.TimeoutSeconds).HasDefaultValue(30);

        builder.HasIndex(x => new { x.OwnerUserId, x.Name });

        builder.HasOne(x => x.OwnerUser)
            .WithMany(x => x.Jobs)
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.ExecutionRequests)
            .WithOne(x => x.Job)
            .HasForeignKey(x => x.JobId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
