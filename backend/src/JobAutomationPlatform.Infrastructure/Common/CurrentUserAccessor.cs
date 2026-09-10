using System.Security.Claims;
using JobAutomationPlatform.Application.Common;
using Microsoft.AspNetCore.Http;

namespace JobAutomationPlatform.Infrastructure.Common;

public sealed class CurrentUserAccessor : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");
            return value is null ? Guid.Empty : Guid.Parse(value);
        }
    }

    public Guid? SessionId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User?.FindFirstValue("sid");
            return value is null ? null : Guid.Parse(value);
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email) ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("email");
}
