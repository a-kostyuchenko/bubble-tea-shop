namespace ServiceDefaults.Domain;

public sealed record Money
{
    private Money()
    {
    }
    
    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; private init; }
    public Currency Currency { get; private init; }

    private static readonly Error NegativeAmount = Error.Problem(
        "Money.NegativeAmount",
        "Amount cannot be negative");

    private static readonly Error InvalidCurrency = Error.Problem(
        "Money.InvalidCurrency",
        "Currency is invalid");
    
    public static Result<Money> Create(decimal amount, Currency? currency)
    {
        if (currency is null)
        {
            return Result.Failure<Money>(Currency.NotSupported);
        }
        
        if (amount < decimal.Zero)
        {
            return Result.Failure<Money>(NegativeAmount);
        }

        if (currency == Currency.None)
        {
            return Result.Failure<Money>(InvalidCurrency);
        }

        return Result.Success(new Money(amount, currency));
    }
    
    public static Money operator +(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new InvalidOperationException("Currencies have to be equal");
        }

        return first with { Amount = first.Amount + second.Amount };
    }
    
    public static Money operator -(Money first, Money second)
    {
        if (first.Currency != second.Currency)
        {
            throw new InvalidOperationException("Currencies have to be equal");
        }

        if (first.Amount < second.Amount)
        {
            throw new InvalidOperationException("Amount cannot be negative");
        }

        return first with { Amount = first.Amount - second.Amount };
    }

    public static Money Zero() => new(decimal.Zero, Currency.None);
    public static Money Zero(Currency currency) => new(decimal.Zero, currency);

    public bool IsZero() => this == Zero(Currency);
    public string Format(IFormatProvider numberFormat) => Currency.Format(Amount, numberFormat);
}
