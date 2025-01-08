using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BubbleTea.Services.Orders.API.Infrastructure.Outbox;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Content)
            .HasMaxLength(2000)
            .HasColumnType("jsonb");
    }
}
