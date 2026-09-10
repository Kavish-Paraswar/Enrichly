using System.Security.Cryptography;
using JobAutomationPlatform.Application.Common;
using JobAutomationPlatform.Application.Dto;
using JobAutomationPlatform.Application.Interfaces;
using JobAutomationPlatform.Domain.Entities;
using JobAutomationPlatform.Infrastructure.Persistence;
using JobAutomationPlatform.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace JobAutomationPlatform.Infrastructure.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IClock _clock;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly SessionSettings _sessionSettings;

    public AuthService(AppDbContext dbContext, IClock clock, IJwtTokenService jwtTokenService, ICurrentUserContext currentUserContext, Microsoft.Extensions.Options.IOptions<SessionSettings> sessionSettings)
    {
        _dbContext = dbContext;
        _clock = clock;
        _jwtTokenService = jwtTokenService;
        _currentUserContext = currentUserContext;
        _sessionSettings = sessionSettings.Value;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        if (await _dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        var utcNow = _clock.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = request.DisplayName.Trim(),
            PasswordHash = PasswordHashing.Hash(request.Password),
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
        };

        var session = CreateSession(user.Id, ipAddress, userAgent, utcNow);
        _dbContext.Users.Add(user);
        _dbContext.SessionTokens.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var (token, expiresAtUtc, _) = _jwtTokenService.Create(user, session);
        return new AuthResponseDto(user.ToDto(), token, expiresAtUtc, session.Id);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (!PasswordHashing.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var utcNow = _clock.UtcNow;
        user.UpdatedAtUtc = utcNow;
        var session = CreateSession(user.Id, ipAddress, userAgent, utcNow);
        _dbContext.SessionTokens.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var (token, expiresAtUtc, _) = _jwtTokenService.Create(user, session);
        return new AuthResponseDto(user.ToDto(), token, expiresAtUtc, session.Id);
    }

    public async Task LogoutAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated || _currentUserContext.SessionId is null)
        {
            throw new UnauthorizedException("You must be signed in to logout.");
        }

        if (_currentUserContext.SessionId != sessionId)
        {
            throw new ConflictException("You can only revoke the active session.");
        }

        var session = await _dbContext.SessionTokens.FirstOrDefaultAsync(x => x.Id == sessionId, cancellationToken)
            ?? throw new NotFoundException("Session not found.");

        session.RevokedAtUtc = _clock.UtcNow;
        session.RevokedReason = "User requested logout.";
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserDto> GetMeAsync(CancellationToken cancellationToken)
    {
        if (!_currentUserContext.IsAuthenticated)
        {
            throw new UnauthorizedException("You must be signed in.");
        }

        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == _currentUserContext.UserId, cancellationToken)
            ?? throw new NotFoundException("Current user was not found.");

        return user.ToDto();
    }

    private SessionToken CreateSession(Guid userId, string? ipAddress, string? userAgent, DateTimeOffset utcNow)
    {
        var sessionId = Guid.NewGuid();
        var tokenId = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
        var expiresAtUtc = utcNow.AddDays(Math.Max(1, _sessionSettings.SessionDays));

        return new SessionToken
        {
            Id = sessionId,
            UserId = userId,
            TokenId = tokenId,
            TokenHash = PasswordHashing.Hash($"{tokenId}:{utcNow:O}"),
            CreatedAtUtc = utcNow,
            ExpiresAtUtc = expiresAtUtc,
            IpAddress = ipAddress,
            UserAgent = userAgent,
        };
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
