using Cart.API.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;

namespace Cart.API.Infrastructure.Database;

internal sealed class CartDbContext(DbContextOptions<CartDbContext> options) : DbContext(options)
{

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Cart);
        
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
    }
}
