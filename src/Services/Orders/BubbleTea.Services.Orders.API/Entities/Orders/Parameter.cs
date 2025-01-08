using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Orders.API.Entities.Orders;

public sealed class Parameter : Entity
{
    private Parameter() : base(Ulid.NewUlid())
    {
    }
    
    public string Name { get; private set; }
    public string Option { get; private set; }
    public Money ExtraPrice { get; private set; }
    
    public static Parameter Create(string name, string option, Money extraPrice)
    {
        return new Parameter
        {
            Name = name,
            Option = option,
            ExtraPrice = extraPrice,
        };
    }
}
