namespace Catalog.API.Infrastructure.Inbox;

public interface IInboxProcessor
{
    Task ProcessAsync();
}
