using JobAutomationPlatform.Application.Common;
using JobAutomationPlatform.Application.Interfaces;
using JobAutomationPlatform.Infrastructure.Persistence;
using JobAutomationPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JobAutomationPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddJobAutomationInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default is required.");

        services.AddSingleton<IClock, SystemClock>();
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IExecutionService, ExecutionService>();
        services.AddScoped<IExecutionQueueService, ExecutionQueueService>();
        services.AddScoped<IJobSchedulerService, JobSchedulerService>();

        return services;
    }
}
