using Cart.API.Entities.Carts;
using Cart.API.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDefaults.Domain;

namespace Cart.API.Infrastructure.Database.Configurations;

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable(TableNames.CartItems);
        
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
            .HasConversion(quantity => quantity.Value, value => new Quantity(value));

        builder.Property(i => i.Size)
            .IsRequired()
            .HasConversion(size => size.Name, name => Size.FromName(name))
            .HasMaxLength(50);
        
        builder.Property(i => i.SugarLevel)
            .IsRequired()
            .HasConversion(level => level.Name, name => SugarLevel.FromName(name))
            .HasMaxLength(50);
        
        builder.Property(i => i.IceLevel)
            .IsRequired()
            .HasConversion(level => level.Name, name => IceLevel.FromName(name))
            .HasMaxLength(50);

        builder.Property(i => i.Temperature)
            .IsRequired()
            .HasConversion(temperature => temperature.Name, name => Temperature.FromName(name))
            .HasMaxLength(50);
    }
}
