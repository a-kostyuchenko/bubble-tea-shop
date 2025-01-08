using BubbleTea.ServiceDefaults.Messaging;

namespace BubbleTea.Services.Payment.Application.Invoices.Commands.Create;

public sealed record CreateInvoiceCommand(
    Guid OrderId,
    string Customer,
    List<InvoiceLineModel> Lines) : ICommand<Guid>;
