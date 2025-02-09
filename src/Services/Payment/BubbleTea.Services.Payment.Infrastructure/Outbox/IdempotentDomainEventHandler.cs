using System.Data.Common;
using BubbleTea.Common.Application.Data;
using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Domain;
using BubbleTea.Common.Infrastructure.Outbox;
using Dapper;

namespace BubbleTea.Services.Payment.Infrastructure.Outbox;

internal sealed class IdempotentDomainEventHandler<TDomainEvent>(
    IDomainEventHandler<TDomainEvent> decorated,
    IDbConnectionFactory dbConnectionFactory) 
    : DomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    public override async Task Handle(
        TDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        var consumer = new OutboxMessageConsumer(domainEvent.Id, decorated.GetType().Name);

        if (await OutboxConsumerExistsAsync(connection, consumer))
        {
            return;
        }

        await decorated.Handle(domainEvent, cancellationToken);
        
        await InsertOutboxConsumerAsync(connection, consumer);
    }

    private static async Task<bool> OutboxConsumerExistsAsync(
        DbConnection connection,
        OutboxMessageConsumer consumer)
    {
        const string sql = 
            $"""
            SELECT EXISTS(
                SELECT 1
                FROM payment.outbox_message_consumers
                WHERE outbox_message_id = @OutboxMessageId AND
                      name = @Name
            )
            """;
        
        return await connection.ExecuteScalarAsync<bool>(sql, consumer);
    }
    
    private static async Task InsertOutboxConsumerAsync(
        DbConnection connection,
        OutboxMessageConsumer consumer)
    {
        const string sql =
            $"""
            INSERT INTO payment.outbox_message_consumers(outbox_message_id, name)
            VALUES (@OutboxMessageId, @Name)
            """;
        
        await connection.ExecuteAsync(sql, consumer);
    }
}
