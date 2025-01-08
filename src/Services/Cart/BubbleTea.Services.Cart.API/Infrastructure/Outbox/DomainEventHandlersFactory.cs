using System.Collections.Concurrent;
using System.Reflection;
using BubbleTea.ServiceDefaults.Messaging;

namespace BubbleTea.Services.Cart.API.Infrastructure.Outbox;

public static class DomainEventHandlersFactory
{
    private static readonly ConcurrentDictionary<string, Type[]> HandlersDictionary = new();

    public static IEnumerable<IDomainEventHandler> GetHandlers(
        Type type,
        IServiceProvider serviceProvider,
        Assembly assembly)
    {
        Type[] domainEventHandlerTypes = HandlersDictionary.GetOrAdd(
            $"{assembly.GetName().Name}-{type.Name}",
            _ =>
            {
                return assembly.GetTypes()
                    .Where(t => t.IsAssignableTo(typeof(IDomainEventHandler<>).MakeGenericType(type)))
                    .ToArray();
            });
        
        foreach (Type domainEventHandlerType in domainEventHandlerTypes)
        {
            object domainEventHandler = serviceProvider.GetRequiredService(domainEventHandlerType);

            yield return (domainEventHandler as IDomainEventHandler)!;
        }
    }
}
