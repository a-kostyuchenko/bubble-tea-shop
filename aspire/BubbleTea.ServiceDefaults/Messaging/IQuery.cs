using BubbleTea.ServiceDefaults.Domain;
using MediatR;

namespace BubbleTea.ServiceDefaults.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
