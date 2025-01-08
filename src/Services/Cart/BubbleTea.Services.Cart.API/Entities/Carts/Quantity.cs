using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Cart.API.Entities.Carts;

public sealed record Quantity
{
    private Quantity()
    {
    }

    private Quantity(int value)
    {
        Value = value;
    }

    public int Value { get; init; }
    
    public static readonly Error MustBeGreaterThanZero = Error.Problem(
        "Quantity.MustBeGreaterThanZero",
        "Quantity must be greater than zero.");
    
    public static Result<Quantity> Create(int value)
    {
        if (value <= 0)
        {
            return Result.Failure<Quantity>(MustBeGreaterThanZero);
        }

        return new Quantity(value);
    }
}
