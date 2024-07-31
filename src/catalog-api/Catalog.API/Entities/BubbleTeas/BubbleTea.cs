using ServiceDefaults.Domain;

namespace Catalog.API.Entities.BubbleTeas;

public sealed class BubbleTea : Entity
{
    private BubbleTea() : base(Ulid.NewUlid())
    {
    }
    
    public string Name { get; private set; }
    public TeaType TeaType { get; private set; }
    // TODO: Add ingredients
    
    public static Result<BubbleTea> Create(string name, TeaType teaType)
    {
        return Result.Success(new BubbleTea
        {
            Name = name,
            TeaType = teaType
        });
    }
}

// public sealed class Size : Enumeration<Size>
#pragma warning disable S125
// {
#pragma warning restore S125
//     public static readonly Size Small = new(1, "small", 0.5m);
//     public static readonly Size Medium = new(2, "medium", 0.7m);
//     public static readonly Size Large = new(3, "large", 1m);
//     public decimal Value { get; private init; }
//
//     private Size(int id, string name, decimal value) : base(id, name)
//     {
//         Value = value;
//     }
// }

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
}

// public sealed class SugarLevel : Enumeration<SugarLevel>
#pragma warning disable S125
// {
#pragma warning restore S125
//     public static readonly SugarLevel Zero = new(1, "zero", 0);
//     public static readonly SugarLevel Fifty = new(2, "fifty", 50);
//     public static readonly SugarLevel Full = new(3, "full", 100);
//     
//     public int Value { get; private init; }
//     
//     private SugarLevel(int id, string name, int value) : base(id, name)
//     {
//         Value = value;
//     }
// }
//
// public sealed class IceLevel : Enumeration<IceLevel>
// {
//     public static readonly IceLevel Zero = new(1, "zero", 0);
//     public static readonly IceLevel Fifty = new(2, "fifty", 50);
//     public static readonly IceLevel Full = new(3, "full", 100);
//     
//     public int Value { get; private init; }
//     
//     private IceLevel(int id, string name, int value) : base(id, name)
//     {
//         Value = value;
//     }
// }
//
// public sealed class Temperature : Enumeration<Temperature>
// {
//     public static readonly Temperature Hot = new(1, "hot");
//     public static readonly Temperature Cold = new(2, "cold");
//     
//     private Temperature(int id, string name) : base(id, name)
//     {
//     }
// }
//
// public static class BubbleTeaDefaults
// {
//     public static Size Size => Size.Medium;
//     public static SugarLevel SugarLevel => SugarLevel.Fifty;
//     public static IceLevel IceLevel => IceLevel.Fifty;
//     public static Temperature Temperature => Temperature.Cold;
// }
//
// public static class BubbleTeaErrors
// {
//     public static readonly Error HotTemperatureWithIce = Error.Problem(
//         "BubbleTea.HotTemperatureWithIce",
//         "Hot bubble tea should not have ice.");
// }
