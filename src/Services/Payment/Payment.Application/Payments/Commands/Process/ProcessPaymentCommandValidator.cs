using FluentValidation;
using Payment.Domain.Payments;

namespace Payment.Application.Payments.Commands.Process;

internal sealed class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
{
    public ProcessPaymentCommandValidator()
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