using ServiceDefaults.Domain;

namespace Catalog.API.Entities.Products;

public static class ProductErrors
{
    public static Error NotFound(Guid productId) => Error.NotFound(
        "Product.NotFound",
        $"The product with the identifier {productId} was not found");
}
