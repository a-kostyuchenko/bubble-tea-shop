using BubbleTea.Common.Application.Messaging;
using BubbleTea.Contracts;

namespace BubbleTea.Services.Payment.Application.Payments.Commands.Process;

public sealed record ProcessPaymentCommand(
    Guid OrderId,
    decimal Amount,
    string Currency,
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string CVV,
    string CardHolderName,
    List<CartItemModel> Items) : ICommand;
