using BubbleTea.Services.Payment.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace BubbleTea.Services.Payment.Infrastructure.Database.Repositories;

internal sealed class PaymentRepository(PaymentDbContext dbContext) : IPaymentRepository
{
    public async Task<Domain.Payments.Payment?> GetAsync(Guid id, CancellationToken cancellationToken = default) => 
        await dbContext.Payments.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public void Insert(Domain.Payments.Payment payment) => 
        dbContext.Payments.Add(payment);
}
