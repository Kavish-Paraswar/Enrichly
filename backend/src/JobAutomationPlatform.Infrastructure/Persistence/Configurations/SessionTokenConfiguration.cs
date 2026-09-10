using JobAutomationPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobAutomationPlatform.Infrastructure.Persistence.Configurations;

public sealed class SessionTokenConfiguration : IEntityTypeConfiguration<SessionToken>
{
    public void Configure(EntityTypeBuilder<SessionToken> builder)
    {
        builder.ToTable("session_tokens");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.TokenId).IsRequired();
        builder.Property(x => x.TokenHash).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ExpiresAtUtc).IsRequired();

        builder.HasIndex(x => x.TokenId).IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}