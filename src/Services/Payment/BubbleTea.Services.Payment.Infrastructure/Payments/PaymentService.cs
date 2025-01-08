using System.Diagnostics.CodeAnalysis;
using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.Services.Payment.Application.Abstractions.Payments;
using BubbleTea.Services.Payment.Domain.Payments;

namespace BubbleTea.Services.Payment.Infrastructure.Payments;

internal sealed class PaymentService : IPaymentService
{
    [SuppressMessage("Security", "CA5394:Do not use insecure randomness")]
    public Task<Result<PaymentResponse>> ChargeAsync(Money amount, PaymentInfo paymentInfo)
    {
        // Simulate a payment gateway response with a 50% success rate
        if (Random.Shared.NextDouble() > 0.5) 
        {
            return Task.FromResult(Result.Success(new PaymentResponse(Guid.CreateVersion7())));
        }
        
        return Task.FromResult(Result.Failure<PaymentResponse>(PaymentErrors.NotEnoughFunds));
    }
}
