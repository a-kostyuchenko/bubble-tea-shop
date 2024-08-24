using ServiceDefaults.Messaging;

namespace Payment.Application.Payments.Commands.Process;

public sealed record ProcessPaymentCommand(
    Guid OrderId,
    decimal Amount,
    string Currency, 
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string CVV,
    string CardHolderName) : ICommand;
