using BubbleTea.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BubbleTea.Services.Orders.API.Entities.Orders;
using BubbleTea.Services.Orders.API.Infrastructure.Database.Constants;

namespace BubbleTea.Services.Orders.API.Infrastructure.Database.Configurations;

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable(
            TableNames.OrderItems,
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Quantity_GreaterThanZero",
                    sql: "quantity > 0");
            });

        builder.HasKey(oi => oi.Id);
        
        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(300);
        
        builder.ComplexProperty(oi => oi.Price, priceBuilder =>
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
        
        builder.HasMany(oi => oi.Parameters)
            .WithOne()
            .HasForeignKey("order_item_id")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
