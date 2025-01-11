using BubbleTea.Common.Application.EventBus;
using BubbleTea.Common.Application.Exceptions;
using BubbleTea.Common.Domain;
using BubbleTea.Contracts;
using MediatR;
using BubbleTea.Services.Payment.Application.Invoices.Commands.Create;

namespace BubbleTea.Services.Payment.Presentation;

internal sealed class PaymentProcessedIntegrationEventHandler(ISender sender)
    : IntegrationEventHandler<PaymentProcessedEvent>
{
    public override async Task Handle(PaymentProcessedEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        var command = new CreateInvoiceCommand(
            integrationEvent.OrderId,
            integrationEvent.Customer, 
            integrationEvent.Items.Select(item => new InvoiceLineModel(
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.Price,
                item.TotalPrice,
                item.Currency)).ToList());

        Result result = await sender.Send(command, cancellationToken);
        
        if (result.IsFailure)
        {
            throw new BubbleTeaShopException(nameof(CreateInvoiceCommand), result.Error);
        }
    }
}
