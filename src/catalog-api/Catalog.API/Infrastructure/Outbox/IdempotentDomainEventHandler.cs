using System.Data.Common;
using Catalog.API.Infrastructure.Database;
using Dapper;
using ServiceDefaults.Domain;
using ServiceDefaults.Messaging;

namespace Catalog.API.Infrastructure.Outbox;

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
                FROM catalog.outbox_message_consumers
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
            INSERT INTO catalog.outbox_message_consumers(outbox_message_id, name)
            VALUES (@OutboxMessageId, @Name)
            """;
        
        await connection.ExecuteAsync(sql, consumer);
    }
}
