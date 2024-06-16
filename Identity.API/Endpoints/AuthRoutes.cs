using Contracts;
using Identity.API.Entities;
using Identity.API.Permissions;
using Identity.API.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Identity.API.Endpoints;

public static class AuthRoutes
{
    public static void AddAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("users/me",
            [HasPermission(Permission.Client)]
            (ClaimsPrincipal claims) =>
        {
            return Results.Ok(new
            {
                Id = $"User Id = {claims.FindFirst(ClaimTypes.NameIdentifier)?.Value}",
                Name = $"User Name = {claims.FindFirst(ClaimTypes.Name)?.Value}",
                Email = $"User Email = {claims.FindFirst(ClaimTypes.Email)?.Value}",
            });
        }).RequireAuthorization();

        app.MapPost("users/register", async (
            RegisterRequest request,
            IJwtService jwtService,
            UserManager<Client> client,
            RoleManager<IdentityRole<Guid>> role) =>
        {
            var newClient = new Client()
            {
                Email = request.Email,
                Name = request.Name,
                UserName = request.Email
            };

            var clientCreated = await client.CreateAsync(newClient, request.Password);

            if (!clientCreated.Succeeded) return Results.BadRequest(clientCreated.Errors.ToHashSet());

            var checkRole = await role.FindByNameAsync("Client");

            if (checkRole is null)
            {
                await role.CreateAsync(new() { Name = "Client" });
            }

            await client.AddToRoleAsync(newClient, "Client");

            var userRole = await client.GetRolesAsync(newClient);

            var token = jwtService.GenerateToken(newClient, userRole.FirstOrDefault());

            return Results.Ok(new AuthenticationResponse(
                newClient.Id,
                token));
        });

        app.MapPost("users/login", async (
            LoginRequest request,
            IJwtService jwtService,
            UserManager<Client> clientManager) =>
        {
            var client = await clientManager.FindByEmailAsync(request.Email);

            if (client is null) return Results.BadRequest("Invalid credentials");

            bool checkUserPassword = await clientManager.CheckPasswordAsync(client, request.Password);

            if (!checkUserPassword) return Results.BadRequest("Invalid credentials");

            var clientRoles = await clientManager.GetRolesAsync(client);

            var token = jwtService.GenerateToken(client, clientRoles.FirstOrDefault());

            return Results.Ok(new AuthenticationResponse(client.Id, token));
        });
    }
}