using JobAutomationPlatform.Api.Middleware;
using JobAutomationPlatform.Application.Common;
using JobAutomationPlatform.Domain.Entities;
using JobAutomationPlatform.Infrastructure;
using JobAutomationPlatform.Infrastructure.Persistence;
using JobAutomationPlatform.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SessionSettings>(builder.Configuration.GetSection("Session"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserAccessor>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Job Automation Platform API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddJobAutomationInfrastructure(builder.Configuration);

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? throw new InvalidOperationException("Jwt settings are required.");
if (string.IsNullOrWhiteSpace(jwtSettings.SigningKey) || jwtSettings.SigningKey.Length < 32)
{
    throw new InvalidOperationException("Jwt__SigningKey must be at least 32 characters.");
}

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var sessionIdClaim = context.Principal?.FindFirst("sid")?.Value;
                if (!Guid.TryParse(sessionIdClaim, out var sessionId))
                {
                    context.Fail("Missing session id.");
                    return;
                }

                var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                var session = await dbContext.SessionTokens.AsNoTracking().FirstOrDefaultAsync(x => x.Id == sessionId, context.HttpContext.RequestAborted);
                if (session is null || session.RevokedAtUtc is not null || session.ExpiresAtUtc <= DateTimeOffset.UtcNow)
                {
                    context.Fail("Session is no longer valid.");
                }
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

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
