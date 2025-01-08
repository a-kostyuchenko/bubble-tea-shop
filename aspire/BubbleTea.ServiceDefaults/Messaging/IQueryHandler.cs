using BubbleTea.ServiceDefaults.Domain;
using MediatR;

namespace BubbleTea.ServiceDefaults.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
