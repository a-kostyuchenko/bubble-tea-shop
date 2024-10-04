using Catalog.API.Entities.Ingredients;
using ServiceDefaults.Domain;

namespace Catalog.API.Entities.Products;

public sealed class Product : Entity
{
    private Product() : base(Ulid.NewUlid())
    {
    }

    private readonly HashSet<Ingredient> _ingredients = [];
    private readonly HashSet<Parameter> _parameters = [];
    
    public string Name { get; private set; }
    public Money Price { get; private set; }
    public Category Category { get; private set; }
    public IReadOnlyCollection<Ingredient> Ingredients => [.. _ingredients];
    public IReadOnlyCollection<Parameter> Parameters => [.. _parameters];
    
    public static Result<Product> Create(string name, Category category, Money? price = null)
    {
        return Result.Success(new Product
        {
            Name = name,
            Category = category,
            Price = price ?? Money.Zero(),
        });
    }
    
    public void Update(string name, Category category, Money price)
    {
        if (Name == name && Category == category && Price == price)
        {
            return;
        }
        
        Name = name;
        Category = category;
        Price = price;
    }
    
    public void AddIngredient(Ingredient ingredient) => _ingredients.Add(ingredient);

    public void RemoveIngredient(Ingredient ingredient) => _ingredients.Remove(ingredient);

    public void AddParameter(Parameter parameter) => _parameters.Add(parameter);

    public void RemoveParameter(Parameter parameter) => _parameters.Remove(parameter);
}
