using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.Services.Payment.Domain.Payments;
using BubbleTea.Services.Payment.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BubbleTea.Services.Payment.Infrastructure.Database.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Domain.Payments.Payment>
{
    public void Configure(EntityTypeBuilder<Domain.Payments.Payment> builder)
    {
        builder.ToTable(TableNames.Payments);
        
        builder.HasKey(p => p.Id);
        
        builder.ComplexProperty(oi => oi.Amount, amountBuilder =>
        {
            amountBuilder.Property(p => p.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");

            amountBuilder.Property(p => p.Currency)
                .IsRequired()
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code))
                .HasMaxLength(3)
                .HasColumnName("currency");
        });

        builder.ComplexProperty(p => p.PaymentInfo, infoBuilder =>
        {
            infoBuilder.Property(i => i.CardNumber)
                .HasMaxLength(PaymentInfo.DefaultCardNumberLength)
                .HasColumnName("card_number");

            infoBuilder.Property(i => i.CVV)
                .HasMaxLength(PaymentInfo.DefaultCvvLength)
                .HasColumnName("cvv");

            infoBuilder.Property(i => i.ExpiryMonth)
                .HasColumnName("expiry_month");
            
            infoBuilder.Property(i => i.ExpiryYear)
                .HasColumnName("expiry_year");
            
            infoBuilder.Property(i => i.CardHolderName)
                .HasColumnName("card_holder_name")
                .HasMaxLength(300);
        });
    }
}
