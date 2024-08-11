using ServiceDefaults.Domain;

namespace Ordering.API.Entities.Orders;

public static class OrderErrors
{
    public static Error NotFound(Guid orderId) => Error.NotFound(
        "Order.NotFound",
        $"The order with the identifier {orderId} was not found");
}
