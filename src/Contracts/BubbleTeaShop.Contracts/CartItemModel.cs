namespace BubbleTeaShop.Contracts;

public sealed record CartItemModel(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal Price,
    decimal TotalPrice,
    string Currency,
    List<ParameterModel> Parameters);

public sealed record ParameterModel(string Name, OptionModel SelectedOption);

public sealed record OptionModel(string Name, decimal ExtraPrice, string Currency);
