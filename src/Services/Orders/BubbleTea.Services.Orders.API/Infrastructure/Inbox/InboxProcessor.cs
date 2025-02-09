using System.Data;
using System.Data.Common;
using BubbleTea.Common.Application.Data;
using BubbleTea.Common.Application.EventBus;
using BubbleTea.Common.Infrastructure.Inbox;
using BubbleTea.Common.Infrastructure.Serialization;
using Dapper;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BubbleTea.Services.Orders.API.Infrastructure.Inbox;

internal sealed class InboxProcessor(
    IDbConnectionFactory dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IOptions<InboxOptions> inboxOptions,
    ILogger<InboxProcessor> logger) : IInboxProcessor
{
    private const string ServiceName = "BubbleTea.Services.Orders.API";
    
    public async Task ProcessAsync()
    {
        logger.LogInformation("{ServiceName} - Beginning to process inbox messages", ServiceName);

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        IReadOnlyList<InboxMessageResponse> inboxMessages = 
            await GetInboxMessagesAsync(connection, transaction);
        
        foreach (InboxMessageResponse inboxMessage in inboxMessages)
        {
            Exception? exception = null;

            try
            {
                IIntegrationEvent integrationEvent = JsonConvert.DeserializeObject<IIntegrationEvent>(
                    inboxMessage.Content,
                    SerializerSettings.Instance)!;

                using IServiceScope scope = serviceScopeFactory.CreateScope();

                IEnumerable<IIntegrationEventHandler> handlers = IntegrationEventHandlersFactory.GetHandlers(
                    integrationEvent.GetType(),
                    scope.ServiceProvider,
                    AssemblyReference.Assembly);

                foreach (IIntegrationEventHandler integrationEventHandler in handlers)
                {
                    await integrationEventHandler.Handle(integrationEvent);
                }
            }
            catch (Exception caughtException)
            {
                logger.LogError(
                    caughtException,
                    "{ServiceName} - Exception while processing inbox message {MessageId}",
                    ServiceName,
                    inboxMessage.Id);

                exception = caughtException;
            }

            await UpdateInboxMessageAsync(connection, transaction, inboxMessage, exception);
        }
        
        await transaction.CommitAsync();

        logger.LogInformation("{ServiceName} - Completed processing inbox messages", ServiceName);
    }
    
    private async Task<IReadOnlyList<InboxMessageResponse>> GetInboxMessagesAsync(
        IDbConnection connection,
        IDbTransaction transaction)
    {
        string sql =
            $"""
             SELECT
                id AS {nameof(InboxMessageResponse.Id)},
                content AS {nameof(InboxMessageResponse.Content)}
             FROM ordering.inbox_messages
             WHERE processed_on_utc IS NULL
             ORDER BY occurred_on_utc
             LIMIT {inboxOptions.Value.BatchSize}
             FOR UPDATE SKIP LOCKED 
             """;

        IEnumerable<InboxMessageResponse> inboxMessages = await connection.QueryAsync<InboxMessageResponse>(
            sql,
            transaction: transaction);

        return inboxMessages.AsList();
    }

    private async Task UpdateInboxMessageAsync(
        IDbConnection connection,
        IDbTransaction transaction,
        InboxMessageResponse inboxMessage,
        Exception? exception)
    {
        const string sql =
            $"""
             UPDATE ordering.inbox_messages
             SET processed_on_utc = @ProcessedOnUtc,
                 error = @Error
             WHERE id = @Id
             """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                inboxMessage.Id,
                ProcessedOnUtc = DateTime.UtcNow,
                Error = exception?.ToString()
            },
            transaction: transaction);
    }
    
    internal sealed record InboxMessageResponse(Guid Id, string Content);
}
