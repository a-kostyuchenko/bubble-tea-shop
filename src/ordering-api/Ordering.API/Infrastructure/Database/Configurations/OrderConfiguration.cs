using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.API.Entities.Orders;
using Ordering.API.Infrastructure.Database.Constants;

namespace Ordering.API.Infrastructure.Database.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(TableNames.Orders);

        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.Customer).HasMaxLength(200);
        
        builder.Property(o => o.Note).HasMaxLength(500);
        
        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion(status => status.Name, name => OrderStatus.FromName(name))
            .HasMaxLength(50);

        builder.HasMany(c => c.Items)
            .WithOne()
            .HasForeignKey("order_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Items).AutoInclude();
    }
}
