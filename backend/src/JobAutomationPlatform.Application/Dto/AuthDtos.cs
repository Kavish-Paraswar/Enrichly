namespace JobAutomationPlatform.Application.Dto;

public sealed record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record AuthResponseDto(
    UserDto User,
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    Guid SessionId);

public sealed class RegisterRequest
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress, System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Email { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string DisplayName { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MinLength(8), System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Password { get; init; } = string.Empty;
}

public sealed class LoginRequest
{
    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.EmailAddress, System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Email { get; init; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required, System.ComponentModel.DataAnnotations.MaxLength(200)]
    public string Password { get; init; } = string.Empty;
}
