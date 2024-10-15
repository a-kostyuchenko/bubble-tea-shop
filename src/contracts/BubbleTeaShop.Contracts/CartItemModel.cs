namespace BubbleTeaShop.Contracts;

public sealed record CartItemModel(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    string Currency);
