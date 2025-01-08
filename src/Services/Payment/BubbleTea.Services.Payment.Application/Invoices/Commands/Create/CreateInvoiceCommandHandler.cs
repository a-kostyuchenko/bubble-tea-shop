using BubbleTea.Services.Payment.Domain.Invoices;
using BubbleTea.ServiceDefaults.Domain;
using BubbleTea.ServiceDefaults.Messaging;
using BubbleTea.Services.Payment.Application.Abstractions.Data;

namespace BubbleTea.Services.Payment.Application.Invoices.Commands.Create;

internal sealed class CreateInvoiceCommandHandler(
    IInvoiceRepository invoiceRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateInvoiceCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = Invoice.Create(request.OrderId, request.Customer, DateTime.UtcNow);
        
        foreach (InvoiceLineModel line in request.Lines)
        {
            Result<Money> moneyResult = Money.Create(line.Price, Currency.FromCode(line.Currency));
            Result<Money> totalPriceResult = Money.Create(line.TotalPrice, Currency.FromCode(line.Currency));

            if (moneyResult.IsFailure)
            {
                return Result.Failure<Guid>(moneyResult.Error);
            }
            
            invoice.Add(line.ProductId, line.Label, line.Quantity, moneyResult.Value, totalPriceResult.Value);
        }
        
        invoiceRepository.Insert(invoice);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(invoice.Id);
    }
}
