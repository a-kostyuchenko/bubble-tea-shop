using Catalog.API.Entities.Parameters;
using Catalog.API.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDefaults.Domain;

namespace Catalog.API.Infrastructure.Database.Configurations;

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
