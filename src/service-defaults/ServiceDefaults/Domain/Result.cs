using System.Diagnostics.CodeAnalysis;

namespace ServiceDefaults.Domain;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }
    
    public static Result<TValue?> Create<TValue>(TValue? value, Error error)
        where TValue : class
        => (value is null ? Failure<TValue>(error) : Success(value))!;
    
    public static Result<TValue?> Create<TValue>(TValue? value)
        where TValue : class
        => (value is null ? Failure<TValue>(Error.NullValue) : Success(value))!;

    public static Result Success() => new(true, Error.None);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Failure<TValue>(Error error) =>
        new(default, false, error);
    
    public static Result Inspect(params Result[] results) => 
        results.Any(r => r.IsFailure) ? Failure(ValidationError.FromResults(results)) : Success();
}

public class Result<TValue>(TValue? value, bool isSuccess, Error error) 
    : Result(isSuccess, error)
{
    [NotNull]
    public TValue Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);

    public static Result<TValue> ValidationFailure(Error error) =>
        new(default, false, error);
}
