using BubbleTea.Common.Application.Messaging;
using BubbleTea.Common.Domain;
using BubbleTea.Common.Presentation.Endpoints;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using BubbleTea.Services.Catalog.API.Entities.Parameters;
using BubbleTea.Services.Catalog.API.Infrastructure.Database;

namespace BubbleTea.Services.Catalog.API.Features.Parameters;

public static class DeleteParameter
{
    public sealed record Command(Guid ParameterId) : ICommand;
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(c => c.ParameterId).NotEmpty();
        }
    }

    internal sealed class CommandHandler(CatalogDbContext dbContext) : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            Parameter? parameter = await dbContext.Parameters
                .FirstOrDefaultAsync(i => i.Id == request.ParameterId, cancellationToken);

            if (parameter is null)
            {
                return Result.Failure(ParameterErrors.NotFound(request.ParameterId));
            }

            dbContext.Remove(parameter);
            
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }

    public sealed class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("parameters/{parameterId:guid}", Handler)
                .WithTags(nameof(Parameter))
                .WithName(nameof(DeleteParameter));
        }

        private static async Task<IResult> Handler(ISender sender, Guid parameterId)
        {
            var command = new Command(parameterId);
            
            Result result = await sender.Send(command);

            return result.Match(Results.NoContent, ApiResults.Problem);
        }
    }
}
