using MediatR;
using ServiceDefaults.Domain;

namespace ServiceDefaults.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
