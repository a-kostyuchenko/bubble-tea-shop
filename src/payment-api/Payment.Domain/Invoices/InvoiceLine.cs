using ServiceDefaults.Domain;

namespace Payment.Domain.Invoices;

public sealed class InvoiceLine : Entity
{
    private InvoiceLine()
    {
    }
    
    public Guid InvoiceId { get; private set; }
    public Guid ProductId { get; set; }
    public string Label { get; private set; } = string.Empty;
    public int Quantity { get; private set; } = 1;
    public Money Price { get; private set; }
    
    public static InvoiceLine Create(Guid invoiceId, string label, int quantity, Money price) =>
        new()
        {
            InvoiceId = invoiceId,
            Label = label,
            Quantity = quantity,
            Price = price
        };
    
    public void Increment(int quantity, Money price)
    {
        Quantity += quantity;
        Price += price;
    }
}
