using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Payment.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error NotEnoughFunds = Error.Problem(
        "Payment.NotEnoughFunds",
        "Not enough funds.");
}
