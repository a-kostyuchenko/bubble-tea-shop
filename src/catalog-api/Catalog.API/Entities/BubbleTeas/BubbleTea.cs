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
