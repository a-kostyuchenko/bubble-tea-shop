namespace Payment.Infrastructure.Outbox;

public interface IOutboxProcessor
{
    Task ProcessAsync();
}
