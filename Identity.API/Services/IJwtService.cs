using Identity.API.Entities;

namespace Identity.API.Services;

public interface IJwtService
{
    string GenerateToken(Client client);
}
