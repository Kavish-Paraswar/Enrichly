namespace JobAutomationPlatform.Application.Common;

public interface ICurrentUserValidator
{
    void EnsureAuthenticated();

    Guid GetUserId();
}
