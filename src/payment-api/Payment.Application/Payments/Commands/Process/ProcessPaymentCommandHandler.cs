using Payment.Application.Abstractions.Data;
using Payment.Application.Abstractions.Payments;
using Payment.Application.Payments.Commands.Create;
using Payment.Domain.Payments;
using ServiceDefaults.Domain;
using ServiceDefaults.Messaging;

namespace Payment.Application.Payments.Commands.Process;

internal sealed class ProcessPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IUnitOfWork unitOfWork,
    IPaymentService paymentService) : ICommandHandler<ProcessPaymentCommand>
{
    public async Task<Result> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
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
        
        var payment = Domain.Payments.Payment.Process(
            request.OrderId,
            paymentResponse.TransactionId,
            moneyResult.Value,
            paymentInfoResult.Value);
        
        paymentRepository.Insert(payment);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
