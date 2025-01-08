using Catalog.API.Entities.Ingredients;
using Catalog.API.Entities.Parameters;
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
    public string Slug { get; private set; }
    public string Description { get; private set; }
    public Guid ImageId { get; private set; }
    public Money Price { get; private set; }
    public Category Category { get; private set; }
    public IReadOnlyCollection<Ingredient> Ingredients => [.. _ingredients];
    public IReadOnlyCollection<Parameter> Parameters => [.. _parameters];
    
    public static Result<Product> Create(
        string name,
        string slug,
        string description,
        Category category,
        Guid imageId,
        Money? price = null)
    {
        return Result.Success(new Product
        {
            Name = name,
            Slug = slug,
            Description = description,
            Category = category,
            ImageId = imageId,
            Price = price ?? Money.Zero(),
        });
    }
    
    public void Update(string description, Category category, Money price)
    {
        if (Category == category && Price == price && Description == description)
        {
            return;
        }
        
        Description = description;
        Category = category;
        Price = price;
    }

    public void UpdateName(string name, string slug)
    {
        if (Name == name && Slug == slug)
        {
            return;
        }
        
        Name = name;
        Slug = slug;
    }
    
    public void AddIngredient(Ingredient ingredient) => _ingredients.Add(ingredient);

    public void RemoveIngredient(Ingredient ingredient) => _ingredients.Remove(ingredient);

    public void AddParameter(Parameter parameter) => _parameters.Add(parameter);

    public void RemoveParameter(Parameter parameter) => _parameters.Remove(parameter);
}
