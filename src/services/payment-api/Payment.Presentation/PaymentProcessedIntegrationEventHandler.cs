using BubbleTeaShop.Contracts;
using MediatR;
using Payment.Application.Invoices.Commands.Create;
using ServiceDefaults.Domain;
using ServiceDefaults.Exceptions;
using ServiceDefaults.Messaging;

namespace Payment.Presentation;

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
                item.Currency)).ToList());

        Result result = await sender.Send(command, cancellationToken);
        
        if (result.IsFailure)
        {
            throw new BubbleTeaShopException(nameof(CreateInvoiceCommand), result.Error);
        }
    }
}
