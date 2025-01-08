using BubbleTea.ServiceDefaults.Domain;
using MediatR;

namespace BubbleTea.ServiceDefaults.Messaging;

public interface ICommand : IRequest<Result>, IBaseCommand;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

public interface IBaseCommand;
