using Payment.Application.Abstractions.Payments;
using Payment.Domain.Payments;
using ServiceDefaults.Domain;

namespace Payment.Infrastructure.Payments;

internal sealed class PaymentService : IPaymentService
{
    public Task<Result<PaymentResponse>> ChargeAsync(Money amount, PaymentInfo paymentInfo)
    {
#pragma warning disable CA5394
        if (Random.Shared.NextDouble() > 0.5) 
#pragma warning restore CA5394
        {
            return Task.FromResult(Result.Success(new PaymentResponse(Guid.CreateVersion7())));
        }
        
        return Task.FromResult(Result.Failure<PaymentResponse>(PaymentErrors.NotEnoughFunds));
    }
}
