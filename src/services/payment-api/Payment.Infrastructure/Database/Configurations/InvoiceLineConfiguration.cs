using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Invoices;
using Payment.Infrastructure.Database.Constants;
using ServiceDefaults.Domain;

namespace Payment.Infrastructure.Database.Configurations;

internal sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable(TableNames.InvoiceLines, tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_Quantity_NotNegative",
                sql: "quantity > 0");
        });

        builder.HasKey(l => new { l.InvoiceId, l.ProductId });

        builder.Property(l => l.Label)
            .HasMaxLength(300);

        builder.ComplexProperty(l => l.Price, priceBuilder =>
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

        builder.ComplexProperty(l => l.TotalPrice, priceBuilder =>
        {
            priceBuilder.Property(p => p.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("total_amount");

            priceBuilder.Property(p => p.Currency)
                .IsRequired()
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasMaxLength(3)
                .HasColumnName("total_currency");
        });
    }
}
