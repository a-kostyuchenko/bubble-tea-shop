using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Orders.API.Entities.Orders;

public sealed class OrderStatus : Enumeration<OrderStatus>
{
    public static readonly OrderStatus Pending = new(1, "pending");
    public static readonly OrderStatus Paid = new(2, "paid");
    public static readonly OrderStatus Processing = new(3, "processing");
    public static readonly OrderStatus Completed = new(4, "completed");
    public static readonly OrderStatus Cancelled = new(5, "cancelled");
    public static readonly OrderStatus PaymentFailed = new(6, "payment_failed");
    
    private OrderStatus()
    {
    }

    private OrderStatus(int id, string name) : base(id, name)
    {
    }
}
