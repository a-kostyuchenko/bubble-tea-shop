using Payment.Domain.Payments;
using ServiceDefaults.Domain;

namespace Payment.Application.Abstractions.Payments;

public interface IPaymentService
{
    Task<Result<PaymentResponse>> ChargeAsync(Money amount, PaymentInfo paymentInfo);
}
