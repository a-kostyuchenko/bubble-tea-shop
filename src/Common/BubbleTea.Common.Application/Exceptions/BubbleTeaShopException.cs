using BubbleTea.Common.Domain;

namespace BubbleTea.Common.Application.Exceptions;

public sealed class BubbleTeaShopException(
    string requestName,
    Error? error = default,
    Exception? innerException = default) : Exception("Application exception", innerException)
{
    public string RequestName { get; } = requestName;
    public Error? Error { get; } = error;
}
