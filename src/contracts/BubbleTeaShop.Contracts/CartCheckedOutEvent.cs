using ServiceDefaults.Messaging;

namespace BubbleTeaShop.Contracts;

public sealed class CartCheckedOutEvent(
    Guid id,
    DateTime occurredOnUtc,
    Guid cartId,
    string customer,
    string? note,
    List<CartItemModel> items) : IntegrationEvent(id, occurredOnUtc)
{
    public Guid CartId { get; init; } = cartId;
    public string Customer { get; set; } = customer;
    public string? Note { get; set; } = note;
    public List<CartItemModel> Items { get; set; } = items;
}

public sealed record CartItemModel(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    string Currency,
    string Size,
    string SugarLevel,
    string IceLevel,
    string Temperature);
