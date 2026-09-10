using JobAutomationPlatform.Application.Dto;

namespace JobAutomationPlatform.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken);

    Task<AuthResponseDto> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken);

    Task LogoutAsync(Guid sessionId, CancellationToken cancellationToken);

    Task<UserDto> GetMeAsync(CancellationToken cancellationToken);
}
