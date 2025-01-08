namespace Payment.Domain.Invoices;

public interface IInvoiceRepository
{
    Task<Invoice?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    void Insert(Invoice invoice);
}
