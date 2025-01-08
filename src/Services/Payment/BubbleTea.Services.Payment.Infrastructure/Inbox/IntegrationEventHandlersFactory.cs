using System.Collections.Concurrent;
using System.Reflection;
using BubbleTea.ServiceDefaults.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace BubbleTea.Services.Payment.Infrastructure.Inbox;

public static class IntegrationEventHandlersFactory
{
    private static readonly ConcurrentDictionary<string, Type[]> HandlersDictionary = new();

    public static IEnumerable<IIntegrationEventHandler> GetHandlers(
        Type type,
        IServiceProvider serviceProvider,
        Assembly assembly)
    {
        Type[] integrationEventHandlerTypes = HandlersDictionary.GetOrAdd(
            $"{assembly.GetName().Name}-{type.Name}",
            _ =>
            {
                return assembly.GetTypes()
                    .Where(t => t.IsAssignableTo(typeof(IIntegrationEventHandler<>).MakeGenericType(type)))
                    .ToArray();
            });

        foreach (Type integrationEventHandlerType in integrationEventHandlerTypes)
        {
            object integrationEventHandler = serviceProvider.GetRequiredService(integrationEventHandlerType);

            yield return (integrationEventHandler as IIntegrationEventHandler)!;
        }
    }
}
