using ServiceDefaults.Domain;

namespace Cart.API.Entities.Carts;

public sealed class CartItem : Entity
{
    private CartItem() : base(Ulid.NewUlid())
    {
    }

    private readonly HashSet<Parameter> _parameters = [];
    
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public Money Price { get; private set; }
    public Quantity Quantity { get; private set; }
    public Money TotalPrice => 
        Price * Quantity.Value +
        Parameters.Aggregate(Money.Zero(), (money, parameter) => money + parameter.SelectedOption.ExtraPrice);
    public IReadOnlyCollection<Parameter> Parameters => [.. _parameters];
    
    public static Result<CartItem> Create(
        Guid productId,
        string productName,
        Money price,
        Quantity quantity)
    {
        return new CartItem
        {
            ProductId = productId,
            ProductName = productName,
            Price = price,
            Quantity = quantity,
        };
    }
    
    public static Result<CartItem> Create(
        Guid productId,
        string productName,
        Money price,
        Quantity quantity,
        HashSet<Parameter> parameters)
    {
        var cartItem = new CartItem
        {
            ProductId = productId,
            ProductName = productName,
            Price = price,
            Quantity = quantity,
        };
        
        cartItem.AddRange(parameters);
        
        return cartItem;
    }
    
    public void Add(Parameter parameter)
    {
        _parameters.Add(parameter);
    }
    
    public void AddRange(HashSet<Parameter> parameters)
    {
        _parameters.UnionWith(parameters);
    }
}
