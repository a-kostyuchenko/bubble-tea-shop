using Cart.API.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Cart.API.Extensions;

internal static class MigrationExtensions
{
    internal static void ApplyMigrations(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        CartDbContext dbContext = scope.ServiceProvider.GetRequiredService<CartDbContext>();

        dbContext.Database.Migrate();
    }
}
