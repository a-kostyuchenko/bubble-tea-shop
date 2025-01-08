namespace BubbleTea.Services.Catalog.API.Infrastructure.Outbox;

public interface IOutboxProcessor
{
    Task ProcessAsync();
}
