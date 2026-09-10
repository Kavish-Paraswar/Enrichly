namespace JobAutomationPlatform.Application.Common;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }

    Guid UserId { get; }

    Guid? SessionId { get; }

    string? Email { get; }
}

