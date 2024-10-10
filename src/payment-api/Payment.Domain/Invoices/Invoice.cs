using ServiceDefaults.Domain;

namespace Payment.Domain.Invoices;

public sealed class Invoice : Entity
{
    private readonly List<InvoiceLine> _lines = [];

    public string Customer { get; set; } = string.Empty;
    public DateTime IssueTime { get; internal set; }
    public DateTime? PaymentTime { get; internal set; }
    public IReadOnlyCollection<InvoiceLine> Lines => [.. _lines];
    public Money TotalAmount => _lines.Aggregate(Money.Zero(), (total, line) => total + line.Price);
    
    public void Add(Guid productId, string label, int quantity, Money price)
    {
        if (_lines.Find(l => l.ProductId == productId) is { } line)
        {
            line.Increment(quantity, price);
            return;
        }
        
        _lines.Add(InvoiceLine.Create(Id, productId, label, quantity, price));
    }
}
