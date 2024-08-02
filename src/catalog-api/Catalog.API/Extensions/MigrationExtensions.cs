using Catalog.API.Database;
using Catalog.API.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Extensions;

internal static class MigrationExtensions
{
    internal static void ApplyMigrations(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        CatalogDbContext dbContext = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        dbContext.Database.Migrate();
    }
}
