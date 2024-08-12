using Catalog.API.Entities.Ingredients;
using Catalog.API.Entities.Products;
using Catalog.API.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Catalog.API.Infrastructure.Database;

internal sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Catalog);
        
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
        
        modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.IsPrimaryKey())
            .ToList()
            .ForEach(p => p.ValueGenerated = ValueGenerated.Never);
    }
}
