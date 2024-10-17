using ServiceDefaults.Domain;

namespace Ordering.API.Entities.Orders;

public sealed class OrderItem : Entity
{
    private OrderItem() : base(Ulid.NewUlid())
    {
    }

    private readonly HashSet<Parameter> _parameters = [];
    
    public string ProductName { get; private set; }
    public Money Price { get; private set; }
    public int Quantity { get; private set; }
    public IReadOnlyCollection<Parameter> Parameters => [.. _parameters];
    
    public static OrderItem Create(string productName, Money price, int quantity)
    {
        return new OrderItem
        {
            ProductName = productName,
            Price = price,
            Quantity = quantity,
        };
    }
    
    public static OrderItem Create(string productName, Money price, int quantity, List<Parameter> parameters)
    {
        var item = new OrderItem
        {
            ProductName = productName,
            Price = price,
            Quantity = quantity,
        };
        
        foreach (Parameter parameter in parameters)
        {
            item.AddParameter(parameter);
        }
        
        return item;
    }
    
    public void AddParameter(string name, string option, Money extraPrice) => 
        _parameters.Add(Parameter.Create(name, option, extraPrice));
    
    public void AddParameter(Parameter parameter) => _parameters.Add(parameter);
}
