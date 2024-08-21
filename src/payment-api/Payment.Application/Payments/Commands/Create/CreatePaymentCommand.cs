using FluentValidation;
using Payment.Application.Abstractions.Data;
using Payment.Application.Abstractions.Payments;
using Payment.Domain.Payments;
using ServiceDefaults.Domain;
using ServiceDefaults.Messaging;

namespace Payment.Application.Payments.Commands.Create;

public sealed record CreatePaymentCommand(
    Guid OrderId,
    decimal Amount,
    string Currency, 
    string CardNumber,
    int ExpiryMonth,
    int ExpiryYear,
    string CVV,
    string CardHolderName) : ICommand;

internal sealed class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(c => c.OrderId).NotEmpty();
        
        RuleFor(c => c.Amount).GreaterThan(0);
        
        RuleFor(c => c.Currency).NotEmpty().MaximumLength(3);
        
        RuleFor(c => c.CardNumber).CreditCard();
        
        RuleFor(c => c.ExpiryMonth).InclusiveBetween(1, 12);
        
        RuleFor(c => c.ExpiryYear).InclusiveBetween(DateTime.UtcNow.Year, DateTime.UtcNow.Year + 10);
        
        RuleFor(c => c.CVV).NotEmpty().MaximumLength(PaymentInfo.DefaultCvvLength);
        
        RuleFor(c => c.CardHolderName).NotEmpty().MaximumLength(300);
    }
}

internal sealed class CreatePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IPaymentService paymentService) : ICommandHandler<CreatePaymentCommand>
{
    public async Task<Result> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
    {
        Result<Money> moneyResult = Money.Create(request.Amount, Currency.FromCode(request.Currency));
        Result<PaymentInfo> paymentInfoResult = PaymentInfo.Create(
            request.CardNumber,
            request.ExpiryMonth,
            request.ExpiryYear,
            request.CVV,
            request.CardHolderName);
        
        var inspection = Result.Inspect(moneyResult, paymentInfoResult);

        if (inspection.IsFailure)
        {
            return inspection;
        }
        
        // This is a simplified version of the actual payment process.
        // In a real-world scenario, you would call a payment gateway API.
        PaymentResponse paymentResponse = await paymentService.ChargeAsync(
            moneyResult.Value,
            paymentInfoResult.Value);
        
        var payment = Domain.Payments.Payment.Create(
            request.OrderId,
            paymentResponse.TransactionId,
            moneyResult.Value,
            paymentInfoResult.Value);
        
        paymentRepository.Insert(payment);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
