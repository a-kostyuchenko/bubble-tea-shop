using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Invoices;
using Payment.Infrastructure.Database.Constants;

namespace Payment.Infrastructure.Database.Configurations;

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
