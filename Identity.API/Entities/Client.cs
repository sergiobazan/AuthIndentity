using Microsoft.AspNetCore.Identity;

namespace Identity.API.Entities;

public class Client : IdentityUser<Guid>
{
    public string? Name { get; set; }
}
