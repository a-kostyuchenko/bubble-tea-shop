namespace Payment.Infrastructure.Inbox;

public interface IInboxProcessor
{
    Task ProcessAsync();
}
