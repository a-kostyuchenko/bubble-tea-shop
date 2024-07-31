using System.Globalization;
using ServiceDefaults.Domain;

namespace Catalog.API.Entities.BubbleTeas;

public sealed class Currency : Enumeration<Currency>
{
    public static readonly Currency None = new(0, "None", "NONE");
    public static readonly Currency Usd = new(1, "US Dollar", "USD");
    public static readonly Currency Eur = new(2, "Euro", "EUR");
    public static readonly Currency Kzt = new(3, "Kazakhstani Tenge", "KZT");

    private static readonly IFormatProvider NumberFormat = new CultureInfo("en-US");
    
    private Currency(int value, string name, string code)
        : base(value, name) =>
        Code = code;
    
    private Currency()
    {
    }
    
    public string Code { get; private set; }
    
    public string Format(decimal amount) =>
        $"{amount.ToString("N2", NumberFormat)} {Code}";
    
    public string Format(decimal amount, IFormatProvider numberFormat) =>
        $"{amount.ToString("N2", numberFormat)} {Code}";

    public static Currency FromCode(string code)
    {
        Currency? currency = GetValues().FirstOrDefault(c => c.Code == code);
        
        if (currency is null)
        {
            throw new InvalidOperationException($"Currency with code {code} not found.");
        }

        return currency;
    }
}
