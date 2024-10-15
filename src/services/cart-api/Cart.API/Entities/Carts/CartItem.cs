using ServiceDefaults.Domain;

namespace Cart.API.Entities.Carts;

public sealed class CartItem : Entity
{
    private CartItem() : base(Ulid.NewUlid())
    {
    }
    
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public Money Price { get; private set; }
    public Quantity Quantity { get; private set; }
    
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
            Quantity = quantity
        };
    }
}
