using ServiceDefaults.Domain;

namespace Cart.API.Entities.Carts.Events;

public sealed class CartCheckedOutDomainEvent(Guid cartId) : DomainEvent
{
    public Guid CartId { get; init; } = cartId;
}
