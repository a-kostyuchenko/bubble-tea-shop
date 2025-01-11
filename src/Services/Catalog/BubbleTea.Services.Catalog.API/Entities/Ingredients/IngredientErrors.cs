using BubbleTea.Common.Domain;

namespace BubbleTea.Services.Catalog.API.Entities.Ingredients;

public static class IngredientErrors
{
    public static Error NotFound(Guid ingredientId) => Error.NotFound(
        "Ingredient.NotFound",
        $"The ingredient with the identifier {ingredientId} was not found");
}
