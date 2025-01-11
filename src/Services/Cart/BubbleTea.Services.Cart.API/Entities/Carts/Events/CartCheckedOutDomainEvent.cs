using BubbleTea.Common.Domain;

namespace BubbleTea.Services.Cart.API.Entities.Carts.Events;

public sealed class CartCheckedOutDomainEvent(
    Guid cartId,
    string cardNumber,
    int expiryMonth,
    int expiryYear,
    string cvv,
    string cardHolderName) : DomainEvent
{
    public Guid CartId { get; init; } = cartId;
    public string CardNumber { get; init; } = cardNumber;
    public int ExpiryMonth { get; init; } = expiryMonth;
    public int ExpiryYear { get; init; } = expiryYear;
    public string CVV { get; init; } = cvv;
    public string CardHolderName { get; init; } = cardHolderName;
}
