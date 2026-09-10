using JobAutomationPlatform.Api.Middleware;
using JobAutomationPlatform.Infrastructure;
using JobAutomationPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddJobAutomationInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var attempts = 0;
    while (true)
    {
        try
        {
            await dbContext.Database.MigrateAsync();
            break;
        }
        catch when (attempts < 9)
        {
            attempts += 1;
            await Task.Delay(TimeSpan.FromSeconds(2));
        }
    }
}

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "api" }));
app.MapGet("/health/db", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
{
    var connected = await dbContext.Database.CanConnectAsync(cancellationToken);
    return connected ? Results.Ok(new { status = "ok", database = "connected" }) : Results.Problem("Database is not reachable.", statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.Run();
