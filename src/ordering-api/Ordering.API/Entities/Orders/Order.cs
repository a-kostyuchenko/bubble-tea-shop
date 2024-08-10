using ServiceDefaults.Domain;

namespace Ordering.API.Entities.Orders;

public sealed class Order : Entity
{
    private Order() : base(Ulid.NewUlid())
    {
    }

    private readonly List<OrderItem> _items = [];
    
    public string Customer { get; private set; }
    public string? Note { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedOnUtc { get; init; }
    public IReadOnlyCollection<OrderItem> Items => [.. _items];
    
    public static Order Create(string customer, string? note)
    {
        return new Order
        {
            Customer = customer,
            Note = note,
            Status = OrderStatus.Pending,
            CreatedOnUtc = DateTime.UtcNow
        };
    }
    
    public void AddItem(string productName, Money price, int quantity, Parameters parameters) => 
        _items.Add(OrderItem.Create(productName, price, quantity, parameters));
}
