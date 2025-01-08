namespace BubbleTea.Services.Payment.Infrastructure.Outbox;

public interface IOutboxProcessor
{
    Task ProcessAsync();
}
