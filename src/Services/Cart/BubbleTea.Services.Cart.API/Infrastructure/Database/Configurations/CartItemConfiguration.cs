using BubbleTea.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BubbleTea.Services.Cart.API.Entities.Carts;
using BubbleTea.Services.Cart.API.Infrastructure.Database.Constants;

namespace BubbleTea.Services.Cart.API.Infrastructure.Database.Configurations;

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable(
            TableNames.CartItems,
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Quantity_GreaterThanZero",
                    sql: "quantity > 0");
            });
        
        builder.HasKey(i => i.Id);
        
        builder.Property(i => i.ProductId)
            .IsRequired();
        
        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(300);
        
        builder.ComplexProperty(b => b.Price, priceBuilder =>
        {
            priceBuilder.Property(p => p.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");

            priceBuilder.Property(p => p.Currency)
                .IsRequired()
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasMaxLength(3)
                .HasColumnName("currency");
        });

        builder.Property(i => i.Quantity)
            .IsRequired()
            .HasConversion(quantity => quantity.Value, value => Quantity.Create(value).Value);
        
        builder.HasMany(i => i.Parameters)
            .WithOne()
            .HasForeignKey("cart_item_id")
            .IsRequired();
        
        builder.Navigation(i => i.Parameters).AutoInclude();
    }
}
