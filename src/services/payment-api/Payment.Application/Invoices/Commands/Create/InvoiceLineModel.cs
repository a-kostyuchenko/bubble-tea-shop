namespace Payment.Application.Invoices.Commands.Create;

public record InvoiceLineModel(Guid ProductId, string Label, int Quantity, decimal Price, string Currency);