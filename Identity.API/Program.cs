using Contracts;
using Identity.API.Database;
using Identity.API.Entities;
using Identity.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string? databaseConnection = builder.Configuration.GetConnectionString("Database") 
    ?? throw new InvalidOperationException("No db connection string provided");

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseNpgsql(databaseConnection));

// Add Identity
builder.Services.AddIdentity<Client, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddRoles<IdentityRole<Guid>>();

// Add JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secretkey"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IJwtService, JwtService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.AddMigrations();
}

app.UseHttpsRedirection();

app.MapGet("/users/me", (ClaimsPrincipal user) =>
{
    if (user.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new { Message = "Yooo", User = user.Identity?.Name });
}).RequireAuthorization();

app.MapPost("/users/register", async (
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

    var token = jwtService.GenerateToken(newClient);

    return Results.Ok(new AuthenticationResponse(
        newClient.Id,
        token));    
});

app.Run();