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
    public Size Size { get; private set; }
    public SugarLevel SugarLevel { get; private set; }
    public IceLevel IceLevel { get; private set; }
    public Temperature Temperature { get; private set; }
    
    public static Result<CartItem> Create(
        Guid productId,
        string productName,
        Money price,
        Quantity quantity,
        Size? size = null,
        SugarLevel? sugarLevel = null,
        IceLevel? iceLevel = null,
        Temperature? temperature = null)
    {
        if (temperature == Temperature.Hot && iceLevel != IceLevel.Zero)
        {
            return Result.Failure<CartItem>(CartItemErrors.HotTemperatureWithIce);
        }
        
        return new CartItem
        {
            ProductId = productId,
            ProductName = productName,
            Price = price,
            Quantity = quantity,
            Size = size ?? CartItemDefaults.Size,
            SugarLevel = sugarLevel ?? CartItemDefaults.SugarLevel,
            IceLevel = iceLevel ?? CartItemDefaults.IceLevel,
            Temperature = temperature ?? CartItemDefaults.Temperature
        };
    }
}
