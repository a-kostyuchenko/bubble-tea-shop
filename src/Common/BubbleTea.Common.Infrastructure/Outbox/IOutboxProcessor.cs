namespace BubbleTea.Common.Infrastructure.Outbox;

public interface IOutboxProcessor
{
    Task ProcessAsync();
}
