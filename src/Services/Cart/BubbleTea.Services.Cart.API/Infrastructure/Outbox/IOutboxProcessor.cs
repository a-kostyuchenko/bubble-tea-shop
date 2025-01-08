namespace BubbleTea.Services.Cart.API.Infrastructure.Outbox;

public interface IOutboxProcessor
{
    Task ProcessAsync();
}
