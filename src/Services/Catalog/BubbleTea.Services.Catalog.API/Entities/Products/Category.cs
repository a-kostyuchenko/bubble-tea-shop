using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Catalog.API.Entities.Products;

public sealed class Category : Enumeration<Category>
{
    public static readonly Category IcedBubbleTea = new(1, "iced_bubble_tea");
    public static readonly Category BubbleTea = new(2, "bubble_tea");
    public static readonly Category Matcha = new(3, "matcha");
    public static readonly Category Dessert = new(4, "dessert");
    
    private Category()
    {
    }

    private Category(int id, string name) : base(id, name)
    {
    }
}
