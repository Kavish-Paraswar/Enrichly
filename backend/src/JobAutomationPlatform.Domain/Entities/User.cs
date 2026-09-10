using System.ComponentModel.DataAnnotations;

namespace JobAutomationPlatform.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<Job> Jobs { get; set; } = new List<Job>();

    public ICollection<SessionToken> Sessions { get; set; } = new List<SessionToken>();
}
