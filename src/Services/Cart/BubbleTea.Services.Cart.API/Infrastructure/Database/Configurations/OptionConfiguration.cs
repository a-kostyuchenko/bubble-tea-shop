using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.Services.Cart.API.Entities.Carts;
using BubbleTea.Services.Cart.API.Infrastructure.Database.Constants;

namespace BubbleTea.Services.Cart.API.Infrastructure.Database.Configurations;

internal sealed class OptionConfiguration : IEntityTypeConfiguration<Option>
{
    public void Configure(EntityTypeBuilder<Option> builder)
    {
        builder.ToTable(TableNames.Options);

        builder.Property(o => o.Name)
            .HasMaxLength(200)
            .IsRequired();
        
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
