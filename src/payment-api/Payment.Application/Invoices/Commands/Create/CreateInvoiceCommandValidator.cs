using FluentValidation;

namespace Payment.Application.Invoices.Commands.Create;

internal sealed class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(c => c.OrderId).NotEmpty();
        RuleFor(c => c.Customer).NotEmpty();
        RuleFor(c => c.Lines).NotEmpty();
        RuleForEach(x => x.Lines)
            .ChildRules(line =>
            {
                line.RuleFor(x => x.ProductId).NotEmpty();
                line.RuleFor(x => x.Label).NotEmpty();
                line.RuleFor(x => x.Quantity).GreaterThan(0);
                line.RuleFor(x => x.Price).GreaterThan(0);
                line.RuleFor(x => x.Currency).NotEmpty();
            });
    }
}