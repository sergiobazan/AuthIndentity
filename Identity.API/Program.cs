using Identity.API.Database;
using Identity.API.Endpoints;
using Identity.API.Entities;
using Identity.API.Options;
using Identity.API.Permissions;
using Identity.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer();

builder.Services.AddAuthorization();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizaitonPolicyProvider>();

builder.Services.ConfigureOptions<JwtOptionsConfiguration>();
builder.Services.ConfigureOptions<JwtBearerConfiguration>();
builder.Services.ConfigureOptions<SwaggerGenOptionsConfiguration>();

builder.Services.AddScoped<IJwtService, JwtService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.AddMigrations();
}

app.UseHttpsRedirection();

app.AddAuthEndpoints();

app.Run();
