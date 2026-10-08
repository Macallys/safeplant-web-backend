using System.IdentityModel.Tokens.Jwt;
using System.Text;
using IAM.Application;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IAM.Infrastructure;

public sealed class JwtAccessTokenReader : IAccessTokenReader
{
    private readonly IamSettings _settings;

    public JwtAccessTokenReader(IOptions<IamSettings> settings)
    {
        _settings = settings.Value;
    }

    public AccessTokenClaims? Read(string token)
    {
        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey)),
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = handler.ValidateToken(token, parameters, out _);
            var accountId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var sessionId = principal.FindFirst("sessionId")?.Value;
            var role = principal.FindFirst("role")?.Value;
            var channel = principal.FindFirst("channel")?.Value;
            if (!Guid.TryParse(accountId, out var account)
                || !Guid.TryParse(sessionId, out var session)
                || role is null
                || channel is null)
            {
                return null;
            }

            return new AccessTokenClaims(account, session, role, channel);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
        {
            return null;
        }
    }
}
