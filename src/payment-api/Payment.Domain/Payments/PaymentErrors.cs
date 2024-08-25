using ServiceDefaults.Domain;

namespace Payment.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error NotEnoughFunds = Error.Problem(
        "Payment.NotEnoughFunds",
        "Not enough funds.");
}
