using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JobAutomationPlatform.Application.Common;
using JobAutomationPlatform.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JobAutomationPlatform.Infrastructure.Security;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAtUtc, string TokenId) Create(User user, SessionToken sessionToken);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly TimeProvider _timeProvider;

    public JwtTokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
        _timeProvider = TimeProvider.System;
    }

    public (string Token, DateTimeOffset ExpiresAtUtc, string TokenId) Create(User user, SessionToken sessionToken)
    {
        var now = _timeProvider.GetUtcNow();
        var expiresAtUtc = now.AddMinutes(_settings.TokenMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("sid", sessionToken.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, sessionToken.TokenId),
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc, sessionToken.TokenId);
    }
}
