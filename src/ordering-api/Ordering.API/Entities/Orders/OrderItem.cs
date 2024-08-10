using ServiceDefaults.Domain;

namespace Ordering.API.Entities.Orders;

public sealed class OrderItem : Entity
{
    private OrderItem() : base(Ulid.NewUlid())
    {
    }
    
    public string ProductName { get; private set; }
    public Money Price { get; private set; }
    public int Quantity { get; private set; }
    public Parameters Parameters { get; private set; }
    
    public static OrderItem Create(string productName, Money price, int quantity, Parameters parameters)
    {
        return new OrderItem
        {
            ProductName = productName,
            Price = price,
            Quantity = quantity,
            Parameters = parameters
        };
    }
}
