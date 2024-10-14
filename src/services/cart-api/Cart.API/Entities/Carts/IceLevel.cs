using ServiceDefaults.Domain;

namespace Cart.API.Entities.Carts;

public sealed class IceLevel : Enumeration<IceLevel>
{
    public static readonly IceLevel Zero = new(1, "zero", 0);
    public static readonly IceLevel Quarter = new(2, "quarter", 25);
    public static readonly IceLevel Fifty = new(3, "fifty", 50);
    public static readonly IceLevel Full = new(4, "full", 100);
    
    public int Value { get; private init; }
    
    private IceLevel(int id, string name, int value) : base(id, name)
    {
        Value = value;
    }
}