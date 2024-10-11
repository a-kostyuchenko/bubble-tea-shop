using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Payment.Application.Abstractions.Data;
using Payment.Domain.Invoices;
using Payment.Infrastructure.Database.Constants;

namespace Payment.Infrastructure.Database;

public sealed class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<Domain.Payments.Payment> Payments { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Payment);
        
        modelBuilder.ApplyConfigurationsFromAssembly(AssemblyReference.Assembly);
        
        modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetProperties())
            .Where(p => p.IsPrimaryKey())
            .ToList()
            .ForEach(p => p.ValueGenerated = ValueGenerated.Never);
    }
}
