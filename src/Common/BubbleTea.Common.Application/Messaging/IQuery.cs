using BubbleTea.Common.Domain;
using MediatR;

namespace BubbleTea.Common.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
