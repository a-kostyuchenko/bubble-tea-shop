using BubbleTea.Common.Domain;

namespace BubbleTea.Services.Catalog.API.Entities.Ingredients;

public sealed class Ingredient : Entity
{
    private Ingredient() : base(Ulid.NewUlid())
    {
    }

    public string Name { get; private set; }

    public static Result<Ingredient> Create(string name)
    {
        return Result.Success(new Ingredient
        {
            Name = name,
        });
    }
    
    public void Update(string name)
    {
        if (Name == name)
        {
            return;
        }
        
        Name = name;
    }
}
