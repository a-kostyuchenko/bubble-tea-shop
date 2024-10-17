using ServiceDefaults.Domain;

namespace Payment.Domain.Invoices;

public sealed class InvoiceLine : Entity
{
    private InvoiceLine() : base(Ulid.NewUlid())
    {
    }
    
    public Guid InvoiceId { get; private set; }
    public Guid ProductId { get; set; }
    public string Label { get; private set; } = string.Empty;
    public int Quantity { get; private set; } = 1;
    public Money Price { get; private set; }
    public Money TotalPrice { get; private set; }
    
    public static InvoiceLine Create(Guid invoiceId, Guid productId, string label, int quantity, Money price, Money totalPrice) =>
        new()
        {
            InvoiceId = invoiceId,
            ProductId = productId,
            Label = label,
            Quantity = quantity,
            Price = price,
            TotalPrice = totalPrice
        };
    
    public void Increment(int quantity, Money price)
    {
        Quantity += quantity;
        Price += price;
        TotalPrice += price * quantity;
    }
}
