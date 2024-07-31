using Catalog.API.Database.Constants;
using Catalog.API.Entities.BubbleTeas;
using Catalog.API.Entities.Ingredients;
using Microsoft.EntityFrameworkCore;

namespace Catalog.API.Database;

internal sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<BubbleTea> BubbleTeas { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Catalog);
        
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }
}
