using BubbleTea.Common.Domain;
using BubbleTea.Services.Payment.Domain.Payments;

namespace BubbleTea.Services.Payment.Application.Abstractions.Payments;

public interface IPaymentService
{
    Task<Result<PaymentResponse>> ChargeAsync(Money amount, PaymentInfo paymentInfo);
}
