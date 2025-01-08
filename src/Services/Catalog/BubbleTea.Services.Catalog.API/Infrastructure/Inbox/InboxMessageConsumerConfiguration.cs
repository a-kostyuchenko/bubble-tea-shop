using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BubbleTea.Services.Catalog.API.Infrastructure.Inbox;

internal sealed class InboxMessageConsumerConfiguration 
    : IEntityTypeConfiguration<InboxMessageConsumer>
{
    public void Configure(EntityTypeBuilder<InboxMessageConsumer> builder)
    {
        builder.ToTable("inbox_message_consumers");

        builder.HasKey(o => new { o.InboxMessageId, o.Name });

        builder.Property(o => o.Name).HasMaxLength(500);
    }
}
