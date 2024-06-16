using Identity.API.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.API.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(Client client, string role)
    {
        var claims = GetClaims(client, role);
        var securityKey = GetSecurityKey();

        var securityToken = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
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
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
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
