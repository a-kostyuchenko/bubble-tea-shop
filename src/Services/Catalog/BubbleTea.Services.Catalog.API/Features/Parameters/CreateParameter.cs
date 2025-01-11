using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Domain;
using BubbleTea.Common.Presentation.Endpoints;
using FluentValidation;
using MediatR;
using BubbleTea.Services.Catalog.API.Entities.Parameters;
using BubbleTea.Services.Catalog.API.Infrastructure.Database;

namespace BubbleTea.Services.Catalog.API.Features.Parameters;

public static class CreateParameter
{
    public sealed record OptionRequest(string Name, double Value, decimal ExtraPrice, string Currency);
    public sealed record Command(string Name, List<OptionRequest> Options) : ICommand<Guid>;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.Name).NotEmpty().MaximumLength(200);

            RuleForEach(c => c.Options)
                .ChildRules(option =>
                {
                    option.RuleFor(o => o.Name).NotEmpty().MaximumLength(200);
                    option.RuleFor(o => o.Value).GreaterThanOrEqualTo(0);
                    option.RuleFor(o => o.ExtraPrice).GreaterThanOrEqualTo(0);
                    option.RuleFor(o => o.Currency).NotEmpty().MaximumLength(3);
                });
        }
    }

    internal sealed class CommandHandler(CatalogDbContext dbContext) : ICommandHandler<Command, Guid>
    {
        public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var parameter = Parameter.Create(request.Name);
            
            foreach ((string name, double value, decimal extraPrice, string currency) in request.Options)
            {
                Result<Money> moneyResult = Money.Create(extraPrice, Currency.FromCode(currency));

                if (moneyResult.IsFailure)
                {
                    return Result.Failure<Guid>(moneyResult.Error);
                }
                
                parameter.AddOption(name, value, moneyResult.Value);
            }
            
            dbContext.Add(parameter);

            await dbContext.SaveChangesAsync(cancellationToken);

            return parameter.Id;
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("parameters", Handler)
                .WithTags(nameof(Parameter))
                .WithName(nameof(CreateParameter));
        }

        private static async Task<IResult> Handler(ISender sender, Request request)
        {
            var command = new Command(request.Name, request.Options);
            
            Result<Guid> result = await sender.Send(command);

            return result.Match(
                parameterId => Results.CreatedAtRoute(nameof(GetParameter), new { parameterId }, parameterId),
                ApiResults.Problem);
        }

        private sealed record Request(string Name)
        {
            public List<OptionRequest> Options { get; init; } = [];
        }
    }
}
