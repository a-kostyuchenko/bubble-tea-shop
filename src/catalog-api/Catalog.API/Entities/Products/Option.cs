using ServiceDefaults.Domain;

namespace Catalog.API.Entities.Products;

public sealed class Option : Entity
{
    private Option() : base(Ulid.NewUlid())
    {
    }
    
    public string Name { get; set; }
    public double Value { get; set; }
    public Money ExtraPrice { get; set; }
    
    public static Option Create(string name, double value, Money extraPrice) =>
        new()
        {
            Name = name,
            Value = value,
            ExtraPrice = extraPrice
        };
}
