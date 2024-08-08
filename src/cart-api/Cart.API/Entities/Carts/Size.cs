using ServiceDefaults.Domain;

namespace Cart.API.Entities.Carts;

public sealed class Size : Enumeration<Size>
{
    public static readonly Size Small = new(1, "small", 0.5);
    public static readonly Size Medium = new(2, "medium", 0.7);
    public static readonly Size Large = new(3, "large", 1);
    public double Value { get; private init; }

    private Size(int id, string name, double value) : base(id, name)
    {
        Value = value;
    }
}