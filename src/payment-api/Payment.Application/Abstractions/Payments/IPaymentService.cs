using Payment.Domain.Payments;
using ServiceDefaults.Domain;

namespace Payment.Application.Abstractions.Payments;

public interface IPaymentService
{
    Task<PaymentResponse> ChargeAsync(Money amount, PaymentInfo paymentInfo);
}
