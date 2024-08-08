namespace Cart.API.Entities.Carts;

public static class CartItemDefaults
{
    public static Size Size => Size.Medium;
    public static SugarLevel SugarLevel => SugarLevel.Quarter;
    public static IceLevel IceLevel => IceLevel.Quarter;
    public static Temperature Temperature => Temperature.Cold;
}
