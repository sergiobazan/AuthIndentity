using Identity.API.Database;
using Microsoft.EntityFrameworkCore;

public static class MigrationExtentions
{
    public static void AddMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();
        using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();
    }
}