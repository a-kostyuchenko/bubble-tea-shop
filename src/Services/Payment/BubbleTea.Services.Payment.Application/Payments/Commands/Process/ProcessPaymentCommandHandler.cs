using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Domain;
using BubbleTea.Services.Payment.Domain.Payments;
using BubbleTea.Services.Payment.Application.Abstractions.Data;
using BubbleTea.Services.Payment.Application.Abstractions.Payments;

namespace BubbleTea.Services.Payment.Application.Payments.Commands.Process;

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
        Result<PaymentResponse> paymentResult = await paymentService.ChargeAsync(
            moneyResult.Value,
            paymentInfoResult.Value);

        if (paymentResult.IsFailure)
        {
            return paymentResult;
        }
        
        var payment = global::BubbleTea.Services.Payment.Domain.Payments.Payment.Process(
            request.OrderId,
            paymentResult.Value.TransactionId,
            moneyResult.Value,
            paymentInfoResult.Value,
            request.Items);
        
        paymentRepository.Insert(payment);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
