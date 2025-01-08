using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Cart.API.Entities.Carts;

public static class CartItemErrors
{
    public static readonly Error HotTemperatureWithIce = Error.Problem(
        "BubbleTea.HotTemperatureWithIce",
        "Hot product should not have ice.");

    public static Error NotFound(Guid itemId) => Error.NotFound(
        "CartItem.NotFound",
        $"The cart item with the identifier {itemId} was not found");
}
