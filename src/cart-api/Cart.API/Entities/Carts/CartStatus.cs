using ServiceDefaults.Domain;

namespace Cart.API.Entities.Carts;

public sealed class CartStatus : Enumeration<CartStatus>
{
    public static readonly CartStatus Draft = new(1, "draft");
    public static readonly CartStatus CheckedOut = new(2, "checked_out");
    public static readonly CartStatus Cancelled = new(3, "cancelled");
    
    private CartStatus()
    {
    }

    private CartStatus(int id, string name) : base(id, name)
    {
    }
}
