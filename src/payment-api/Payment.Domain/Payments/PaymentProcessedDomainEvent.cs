using BubbleTeaShop.Contracts;
using ServiceDefaults.Domain;

namespace Payment.Domain.Payments;

public sealed class PaymentProcessedDomainEvent(
    Guid paymentId,
    Guid orderId,
    string customer,
    List<CartItemModel> items) : DomainEvent
{
    public Guid PaymentId { get; init; } = paymentId;
    public Guid OrderId { get; init; } = orderId;
    public string Customer { get; init; } = customer;
    public List<CartItemModel> Items { get; init; } = items;
}
