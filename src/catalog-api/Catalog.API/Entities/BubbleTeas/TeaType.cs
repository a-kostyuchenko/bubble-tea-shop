using ServiceDefaults.Domain;

namespace Catalog.API.Entities.BubbleTeas;

public sealed class TeaType : Enumeration<TeaType>
{
    public static readonly TeaType Black = new(1, "black");
    public static readonly TeaType Green = new(2, "green");
    public static readonly TeaType Oolong = new(3, "oolong");
    public static readonly TeaType White = new(4, "white");
    public static readonly TeaType Herbal = new(5, "herbal");

    private TeaType(int id, string name) : base(id, name)
    {
    }

    private TeaType()
    {
    }
}
