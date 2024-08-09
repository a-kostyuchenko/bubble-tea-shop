namespace Ordering.API.Infrastructure.Outbox;

public interface IOutboxProcessor
{
    Task ProcessAsync();
}
