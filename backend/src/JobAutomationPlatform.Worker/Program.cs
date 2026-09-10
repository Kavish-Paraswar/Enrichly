using JobAutomationPlatform.Infrastructure;
using JobAutomationPlatform.Infrastructure.Persistence;
using JobAutomationPlatform.Worker.Services;
using Microsoft.EntityFrameworkCore;
using JobAutomationPlatform.Application.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddJobAutomationInfrastructure(builder.Configuration);
builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("Worker"));
builder.Services.AddHttpClient<IJobRunner, HttpJobRunner>();
builder.Services.AddHostedService<ExecutionProcessorBackgroundService>();
builder.Services.AddHostedService<JobSchedulerBackgroundService>();
builder.Services.AddHostedService<StaleExecutionReaperBackgroundService>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var attempts = 0;
    while (true)
    {
        try
        {
            await dbContext.Database.CanConnectAsync();
            break;
        }
        catch when (attempts < 9)
        {
            attempts += 1;
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}

await host.RunAsync();
