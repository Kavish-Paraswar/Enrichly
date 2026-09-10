using System.ComponentModel.DataAnnotations;

namespace JobAutomationPlatform.Domain.Entities;

public sealed class SessionToken
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public User? User { get; set; }

    [MaxLength(64)]
    public string TokenId { get; set; } = string.Empty;

    [MaxLength(500)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }

    [MaxLength(200)]
    public string? RevokedReason { get; set; }

    [MaxLength(200)]
    public string? IpAddress { get; set; }

    [MaxLength(300)]
    public string? UserAgent { get; set; }
}
