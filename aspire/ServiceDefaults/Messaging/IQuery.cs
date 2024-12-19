using MediatR;
using ServiceDefaults.Domain;

namespace ServiceDefaults.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
