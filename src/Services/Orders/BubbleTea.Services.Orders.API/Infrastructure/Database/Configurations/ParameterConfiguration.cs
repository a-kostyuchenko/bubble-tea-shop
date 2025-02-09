using BubbleTea.Common.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BubbleTea.Services.Orders.API.Entities.Orders;
using BubbleTea.Services.Orders.API.Infrastructure.Database.Constants;

namespace BubbleTea.Services.Orders.API.Infrastructure.Database.Configurations;

internal sealed class ParameterConfiguration : IEntityTypeConfiguration<Parameter>
{
    public void Configure(EntityTypeBuilder<Parameter> builder)
    {
        builder.ToTable(TableNames.Parameters);

        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(300);
        
        builder.Property(p => p.Option)
            .IsRequired()
            .HasMaxLength(300);
        
        builder.ComplexProperty(b => b.ExtraPrice, priceBuilder =>
        {
            priceBuilder.Property(p => p.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("extra_price");

            priceBuilder.Property(p => p.Currency)
                .IsRequired()
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasMaxLength(3)
                .HasColumnName("currency");
        });
    }
}
