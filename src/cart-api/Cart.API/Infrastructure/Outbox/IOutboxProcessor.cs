namespace Cart.API.Infrastructure.Outbox;

public interface IOutboxProcessor
{
    Task ProcessAsync();
}
