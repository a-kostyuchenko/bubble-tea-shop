using System.Data.Common;
using BubbleTea.Common.Application.Data;
using BubbleTea.Common.Application.EventBus;
using BubbleTea.Common.Infrastructure.Inbox;
using BubbleTea.Common.Infrastructure.Serialization;
using Dapper;
using MassTransit;
using Newtonsoft.Json;

namespace BubbleTea.Services.Orders.API.Infrastructure.Inbox;

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
            INSERT INTO ordering.inbox_messages(id, type, content, occurred_on_utc)
            VALUES (@Id, @Type, @Content::json, @OccurredOnUtc)
            """;

        await connection.ExecuteAsync(sql, inboxMessage);
    }
}
