namespace Ordering.API.Infrastructure.Inbox;

public interface IInboxProcessor
{
    Task ProcessAsync();
}
