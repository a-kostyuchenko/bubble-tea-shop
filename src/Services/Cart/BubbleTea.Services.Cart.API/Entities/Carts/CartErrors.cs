using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Cart.API.Entities.Carts;

public static class CartErrors
{
    public static readonly Error CustomerIsMissing = Error.Problem(
        "ShoppingCart.CustomerIsMissing",
        "Customer is required to create a cart.");

    public static readonly Error EmptyCart = Error.Problem(
        "ShoppingCart.EmptyCart",
        "Cannot check out an empty cart.");

    public static readonly Error AlreadyCheckedOut = Error.Problem(
        "ShoppingCart.AlreadyCheckedOut",
        "The cart has already been checked out.");

    public static readonly Error Cancelled = Error.Problem(
        "ShoppingCart.Cancelled",
        "The cart has been cancelled.");

    public static Error NotFound(Guid cartId) => Error.NotFound(
        "ShoppingCart.NotFound",
        $"The cart with the identifier {cartId} was not found");
}
