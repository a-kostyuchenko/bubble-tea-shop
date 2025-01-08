using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Catalog.API.Entities.Products;

public static class ProductErrors
{
    public static Error NotFound(Guid productId) => Error.NotFound(
        "Product.NotFound",
        $"The product with the identifier {productId} was not found");
    
    public static Error NotFound(string slug) => Error.NotFound(
        "Product.NotFound",
        $"The product with the identifier {slug} was not found");
}
