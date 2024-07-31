using ServiceDefaults.Domain;

namespace Catalog.API.Entities.Ingredients;

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
}
