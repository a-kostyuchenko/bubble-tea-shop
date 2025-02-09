using BubbleTea.Common.Domain;

namespace BubbleTea.Services.Orders.API.Entities.Orders;

public static class OrderErrors
{
    public static Error NotFound(Guid orderId) => Error.NotFound(
        "Order.NotFound",
        $"The order with the identifier {orderId} was not found");

    public static Error InvalidStatus(OrderStatus status) => Error.Problem(
        "Order.InvalidStatus",
        $"The order status {status.Name} is invalid");
}
