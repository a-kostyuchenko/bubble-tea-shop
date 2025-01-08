using BubbleTea.Services.Payment.Domain.Invoices;
using BubbleTea.Services.Payment.Infrastructure.Database.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BubbleTea.Services.Payment.Infrastructure.Database.Configurations;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable(TableNames.Invoices);
        
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Customer)
            .HasMaxLength(300)
            .IsRequired();

        builder.HasMany(i => i.Lines)
            .WithOne()
            .HasForeignKey(i => i.InvoiceId)
            .IsRequired();
    }
}
