using ServiceDefaults.Domain;

namespace Cart.API.Entities.Carts;

public sealed class Temperature : Enumeration<Temperature>
{
    public static readonly Temperature Hot = new(1, "hot");
    public static readonly Temperature Cold = new(2, "cold");
    
    private Temperature(int id, string name) : base(id, name)
    {
    }
}