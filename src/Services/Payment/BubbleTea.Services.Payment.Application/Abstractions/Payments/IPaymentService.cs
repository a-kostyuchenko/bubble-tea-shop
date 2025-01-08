using BubbleTea.Services.Payment.Domain.Payments;
using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Payment.Application.Abstractions.Payments;

public interface IPaymentService
{
    Task<Result<PaymentResponse>> ChargeAsync(Money amount, PaymentInfo paymentInfo);
}
