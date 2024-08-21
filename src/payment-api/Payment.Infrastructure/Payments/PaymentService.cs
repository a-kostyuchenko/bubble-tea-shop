using Payment.Application.Abstractions.Payments;
using Payment.Domain.Payments;
using ServiceDefaults.Domain;

namespace Payment.Infrastructure.Payments;

internal sealed class PaymentService : IPaymentService
{
    public Task<PaymentResponse> ChargeAsync(Money amount, PaymentInfo paymentInfo) => 
        Task.FromResult(new PaymentResponse(Guid.NewGuid()));
}
