using BubbleTea.Services.Payment.Domain.Invoices;
using Microsoft.EntityFrameworkCore;

namespace BubbleTea.Services.Payment.Infrastructure.Database.Repositories;

internal sealed class InvoiceRepository(PaymentDbContext dbContext) : IInvoiceRepository
{
    public async Task<Invoice?> GetAsync(Guid id, CancellationToken cancellationToken = default) => 
        await dbContext.Invoices.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Insert(Invoice invoice) => 
        dbContext.Invoices.Add(invoice);
}
