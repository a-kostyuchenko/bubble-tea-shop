using BubbleTea.Common.Application.EventBus;

namespace BubbleTea.Contracts;

public sealed class CartCheckedOutEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid cartId,
    string customer,
    string? note,
    string cardNumber,
    int expiryMonth,
    int expiryYear,
    string cvv,
    string cardHolderName,
    decimal totalAmount,
    string currency,
    List<CartItemModel> items) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid CartId { get; init; } = cartId;
    public string Customer { get; init; } = customer;
    public string? Note { get; init; } = note;
    public string CardNumber { get; init; } = cardNumber;
    public int ExpiryMonth { get; init; } = expiryMonth;
    public int ExpiryYear { get; init; } = expiryYear;
    public string CVV { get; init; } = cvv;
    public string CardHolderName { get; init; } = cardHolderName;
    public decimal TotalAmount { get; init; } = totalAmount;
    public string Currency { get; init; } = currency;
    public List<CartItemModel> Items { get; init; } = items;
}
