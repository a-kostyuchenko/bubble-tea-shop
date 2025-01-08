using BubbleTea.ServiceDefaults.Domain;

namespace BubbleTea.Services.Cart.API.Entities.Carts;

public sealed record PaymentInfo
{
    private PaymentInfo()
    {
    }

    private PaymentInfo(string cardNumber, int expiryMonth, int expiryYear, string cvv, string cardHolderName)
    {
        CardNumber = cardNumber;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        CVV = cvv;
        CardHolderName = cardHolderName;
    }
    
    public const int DefaultCvvLength = 3;
    public const int DefaultCardNumberLength = 16;
    
    public static readonly Error InvalidExpiryMonth = Error.Problem(
        "PaymentInfo.InvalidExpiryMonth",
        "Invalid expiry month.");

    public static readonly Error InvalidExpiryYear = Error.Problem(
        "PaymentInfo.InvalidExpiryYear",
        "Invalid expiry year.");

    public static readonly Error CardExpired = Error.Problem(
        "PaymentInfo.CardExpired",
        "Card has expired.");

    public static readonly Error InvalidCvv = Error.Problem(
        "PaymentInfo.InvalidCvv",
        "Invalid CVV.");

    public static readonly Error InvalidCardNumber = Error.Problem(
        "PaymentInfo.InvalidCardNumber",
        "Invalid card number.");
    
    public string CardNumber { get; init; }
    public int ExpiryMonth { get; init; }
    public int ExpiryYear { get; init; }
    public string CVV { get; init; }
    public string CardHolderName { get; init; }

    public static Result<PaymentInfo> Create(
        string cardNumber,
        int expiryMonth,
        int expiryYear,
        string cvv,
        string cardHolderName)
    {
        if (expiryMonth is < 1 or > 12)
        {
            return Result.Failure<PaymentInfo>(InvalidExpiryMonth);
        }

        if (expiryYear < DateTime.UtcNow.Year)
        {
            return Result.Failure<PaymentInfo>(InvalidExpiryYear);
        }

        if (DateOnly.FromDateTime(DateTime.UtcNow) > new DateOnly(expiryYear, expiryMonth, 1))
        {
            return Result.Failure<PaymentInfo>(CardExpired);
        }
        
        if (cvv.Length != DefaultCvvLength)
        {
            return Result.Failure<PaymentInfo>(InvalidCvv);
        }

        if (cardNumber.Length != DefaultCardNumberLength)
        {
            return Result.Failure<PaymentInfo>(InvalidCardNumber);
        }

        return Result.Success(new PaymentInfo(cardNumber, expiryMonth, expiryYear, cvv, cardHolderName));
    }
    
    public static readonly PaymentInfo Empty = new();
}
