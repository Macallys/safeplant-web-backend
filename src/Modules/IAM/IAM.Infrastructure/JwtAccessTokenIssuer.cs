using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IAM.Application;
using IAM.Domain;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IAM.Infrastructure;

public sealed class JwtAccessTokenIssuer : IAccessTokenIssuer
{
    private readonly IamSettings _settings;

    public JwtAccessTokenIssuer(IOptions<IamSettings> settings)
    {
        _settings = settings.Value;
    }

    public string Issue(UserAccount account, Session session)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim("role", account.Role.ToString()),
                new Claim("channel", session.Channel.ToString()),
                new Claim("sessionId", session.Id.ToString())
            ],
            expires: session.ExpiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
