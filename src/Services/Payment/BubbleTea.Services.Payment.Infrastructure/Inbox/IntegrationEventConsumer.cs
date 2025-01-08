using System.Data.Common;
using BubbleTea.ServiceDefaults.Messaging;
using Dapper;
using MassTransit;
using Newtonsoft.Json;
using BubbleTea.Services.Payment.Application.Abstractions.Data;
using BubbleTea.Services.Payment.Infrastructure.Serialization;

namespace BubbleTea.Services.Payment.Infrastructure.Inbox;

internal sealed class IntegrationEventConsumer<TIntegrationEvent>(
    IDbConnectionFactory dbConnectionFactory) : IConsumer<TIntegrationEvent>
where TIntegrationEvent : IntegrationEvent
{
    public async Task Consume(ConsumeContext<TIntegrationEvent> context)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
        
        TIntegrationEvent integrationEvent = context.Message;

        var inboxMessage = new InboxMessage
        {
            Id = integrationEvent.Id,
            Type = integrationEvent.GetType().Name,
            Content = JsonConvert.SerializeObject(integrationEvent, SerializerSettings.Instance),
            OccurredOnUtc = integrationEvent.OccurredOnUtc
        };
        
        const string sql =
            """
            INSERT INTO payment.inbox_messages(id, type, content, occurred_on_utc)
            VALUES (@Id, @Type, @Content::json, @OccurredOnUtc)
            """;

        await connection.ExecuteAsync(sql, inboxMessage);
    }
}
