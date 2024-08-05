using Catalog.API.Entities.Ingredients;
using ServiceDefaults.Domain;

namespace Catalog.API.Entities.BubbleTeas;

public sealed class BubbleTea : Entity
{
    private BubbleTea() : base(Ulid.NewUlid())
    {
    }

    private readonly HashSet<Ingredient> _ingredients = [];
    
    public string Name { get; private set; }
    public TeaType TeaType { get; private set; }
    public Money Price { get; private set; }
    public IReadOnlyCollection<Ingredient> Ingredients => _ingredients.ToList();
    
    public static Result<BubbleTea> Create(string name, TeaType teaType, Money? price = null)
    {
        return Result.Success(new BubbleTea
        {
            Name = name,
            TeaType = teaType,
            Price = price ?? Money.Zero(),
        });
    }
    
    public void Update(string name, TeaType teaType, Money price)
    {
        if (Name == name && TeaType == teaType && Price == price)
        {
            return;
        }
        
        Name = name;
        TeaType = teaType;
        Price = price;
    }
    
    public void AddIngredient(Ingredient ingredient)
    {
        _ingredients.Add(ingredient);
    }
    
    public void RemoveIngredient(Ingredient ingredient)
    {
        _ingredients.Remove(ingredient);
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
