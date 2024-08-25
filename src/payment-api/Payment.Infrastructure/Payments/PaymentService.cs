using System.Security.Cryptography;
using Payment.Application.Abstractions.Payments;
using Payment.Domain.Payments;
using ServiceDefaults.Domain;

namespace Payment.Infrastructure.Payments;

internal sealed class PaymentService : IPaymentService
{
    public Task<Result<PaymentResponse>> ChargeAsync(Money amount, PaymentInfo paymentInfo)
    {
        if (RandomNumberGenerator.GetInt32(100) < 50)
        {
            return Task.FromResult(Result.Failure<PaymentResponse>(PaymentErrors.NotEnoughFunds));
        }
        
        return Task.FromResult(Result.Success(new PaymentResponse(Guid.NewGuid())));
    }
}
