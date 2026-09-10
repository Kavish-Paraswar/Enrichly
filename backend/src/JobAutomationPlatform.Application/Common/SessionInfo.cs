namespace JobAutomationPlatform.Application.Common;

public sealed record SessionInfo(Guid SessionId, Guid UserId, string Email, DateTimeOffset ExpiresAtUtc);
