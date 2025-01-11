using BubbleTea.Common.Application.EventBus;

namespace BubbleTea.Contracts;

public sealed class PaymentProcessedEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid orderId,
    Guid paymentId,
    string customer,
    List<CartItemModel> items) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid OrderId { get; init; } = orderId;
    public Guid PaymentId { get; init; } = paymentId;
    public string Customer { get; init; } = customer;
    public List<CartItemModel> Items { get; init; } = items;
}
