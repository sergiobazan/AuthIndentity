using Identity.API.Entities;
using Identity.API.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.API.Services;

public class JwtService : IJwtService
{
    private readonly JwtOptions _jwtOptions;

    public JwtService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }

    public string GenerateToken(Client client, string role)
    {
        var claims = GetClaims(client, role);
        var securityKey = GetSecurityKey();

        var securityToken = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            null,
            DateTime.UtcNow.AddMinutes(5),
            securityKey);

        var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

        return token;
    }

    private SigningCredentials GetSecurityKey()
    {
        return new(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key)),
            SecurityAlgorithms.HmacSha256);
    }

    public Claim[] GetClaims(Client client, string role)
    {
        return
        [
            new(JwtRegisteredClaimNames.Sub, client.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, client.Email!),
            new(ClaimTypes.Name, client.UserName!),
            new(ClaimTypes.Role, role)
        ];
    }
}
