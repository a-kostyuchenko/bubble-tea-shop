using BubbleTea.Common.Domain;

namespace BubbleTea.Services.Cart.API.Entities.Carts;

public sealed class Option : Entity
{
    private Option() : base(Ulid.NewUlid())
    {
    }
    
    public string Name { get; private set; }
    public double Value { get; private set; }
    public Money ExtraPrice { get; private set; }
    
    public static Option Create(string name, double value, Money extraPrice) =>
        new()
        {
            Name = name,
            Value = value,
            ExtraPrice = extraPrice
        };
}
