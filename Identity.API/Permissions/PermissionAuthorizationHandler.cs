using Identity.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Identity.API.Permissions;

public class PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        string? clientId = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(clientId, out Guid parsedClientId))
        {
            return;
        }

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        var clientManager = scope.ServiceProvider.GetRequiredService<UserManager<Client>>();

        var client = await clientManager.FindByIdAsync(clientId);

        if (client is null) return;

        var clientRoles = await clientManager.GetRolesAsync(client);

        HashSet<string> roles = clientRoles.ToHashSet();

        if (roles.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
