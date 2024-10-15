using ServiceDefaults.Domain;

namespace Cart.API.Entities.Carts;

public sealed class Parameter : Entity
{
    private Parameter() : base(Ulid.NewUlid())
    {
    }
    
    public string Name { get; private set; }
    public Option SelectedOption { get; private set; }
    
    public static Parameter Create(string name, string optionName, double value, Money extraPrice) =>
        new()
        {
            Name = name,
            SelectedOption = Option.Create(optionName, value, extraPrice)
        };
}