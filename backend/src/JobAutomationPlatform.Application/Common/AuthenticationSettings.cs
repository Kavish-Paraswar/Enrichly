namespace JobAutomationPlatform.Application.Common;

public sealed class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SigningKey { get; set; } = string.Empty;

    public int TokenMinutes { get; set; } = 720;
}

public sealed class SessionSettings
{
    public int SessionDays { get; set; } = 7;
}
