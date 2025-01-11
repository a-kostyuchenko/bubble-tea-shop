using BubbleTea.Common.Domain;

namespace BubbleTea.Services.Payment.Domain.Invoices;

public sealed class Invoice : Entity
{
    private Invoice() : base(Ulid.NewUlid())
    {
    }
    
    private readonly List<InvoiceLine> _lines = [];

    public string Customer { get; private set; } = string.Empty;
    public Guid OrderId { get; private set; }
    public DateTime IssueTime { get; private set; }
    public DateTime? PaymentTime { get; private set; }
    public IReadOnlyCollection<InvoiceLine> Lines => [.. _lines];
    public Money TotalAmount => _lines.Aggregate(Money.Zero(), (total, line) => total + line.Price);
    
    public static Invoice Create(Guid orderId, string customer, DateTime issueTime, DateTime paymentTime)
    {
        var invoice = new Invoice
        {
            Customer = customer,
            IssueTime = issueTime,
            PaymentTime = paymentTime,
            OrderId = orderId
        };
        
        invoice.Raise(new InvoiceCreatedDomainEvent(invoice.Id, invoice.OrderId));

        return invoice;
    }
    
    public static Invoice Create(Guid orderId, string customer, DateTime issueTime)
    {
        var invoice = new Invoice
        {
            Customer = customer,
            IssueTime = issueTime,
            OrderId = orderId
        };
        
        invoice.Raise(new InvoiceCreatedDomainEvent(invoice.Id, invoice.OrderId));

        return invoice;
    }
    
    public void Add(Guid productId, string label, int quantity, Money price, Money totalPrice)
    {
        if (_lines.Find(l => l.ProductId == productId) is { } line)
        {
            line.Increment(quantity, price);
            return;
        }
        
        _lines.Add(InvoiceLine.Create(Id, productId, label, quantity, price, totalPrice));
    }
}
