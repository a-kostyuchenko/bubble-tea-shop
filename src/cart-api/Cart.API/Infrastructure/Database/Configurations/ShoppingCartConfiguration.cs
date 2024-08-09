using Cart.API.Entities.Carts;
using Cart.API.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cart.API.Infrastructure.Database.Configurations;

internal sealed class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
{
    public void Configure(EntityTypeBuilder<ShoppingCart> builder)
    {
        builder.ToTable(TableNames.ShoppingCarts);
        
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Customer).HasMaxLength(200);
        
        builder.Property(c => c.Note).HasMaxLength(500);
        
        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion(status => status.Name, name => CartStatus.FromName(name))
            .HasMaxLength(50);

        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey("cart_id");

        builder.Navigation(c => c.Items).AutoInclude();
    }
}
