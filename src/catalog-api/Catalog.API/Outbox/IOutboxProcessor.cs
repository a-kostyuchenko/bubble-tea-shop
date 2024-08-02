namespace Catalog.API.Outbox;

public interface IOutboxProcessor
{
    Task ProcessAsync();
}
