using JobAutomationPlatform.Domain.Enums;
using JobAutomationPlatform.Domain.Rules;

namespace JobAutomationPlatform.Tests;

public sealed class ExecutionStateRulesTests
{
    [Fact]
    public void CanClaim_WhenQueuedAndReady_ReturnsTrue()
    {
        var canClaim = ExecutionStateRules.CanClaim(ExecutionRequestStatus.Queued, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow);

        Assert.True(canClaim);
    }

    [Fact]
    public void CanRetry_WhenFailed_ReturnsTrue()
    {
        Assert.True(ExecutionStateRules.CanRetry(ExecutionRequestStatus.Failed));
    }

    [Fact]
    public void IsActive_WhenSucceeded_ReturnsFalse()
    {
        Assert.False(ExecutionStateRules.IsActive(ExecutionRequestStatus.Succeeded));
    }
}
