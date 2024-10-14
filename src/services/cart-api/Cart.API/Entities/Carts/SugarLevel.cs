using ServiceDefaults.Domain;

namespace Cart.API.Entities.Carts;

public sealed class SugarLevel : Enumeration<SugarLevel>
{
    public static readonly SugarLevel Zero = new(1, "zero", 0);
    public static readonly SugarLevel Quarter = new(2, "quarter", 25);
    public static readonly SugarLevel Fifty = new(3, "fifty", 50);
    public static readonly SugarLevel Full = new(4, "full", 100);
    
    public int Value { get; private init; }
    
    private SugarLevel(int id, string name, int value) : base(id, name)
    {
        Value = value;
    }
}