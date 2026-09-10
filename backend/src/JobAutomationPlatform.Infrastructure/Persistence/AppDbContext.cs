using JobAutomationPlatform.Domain.Entities;
using JobAutomationPlatform.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace JobAutomationPlatform.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Job> Jobs => Set<Job>();

    public DbSet<User> Users => Set<User>();

    public DbSet<SessionToken> SessionTokens => Set<SessionToken>();

    public DbSet<ExecutionRequest> ExecutionRequests => Set<ExecutionRequest>();

    public DbSet<ExecutionAttempt> ExecutionAttempts => Set<ExecutionAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");
        modelBuilder.UseSnakeCaseNamingConvention();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
