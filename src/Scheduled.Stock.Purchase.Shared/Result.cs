namespace Scheduled.Stock.Purchase.Shared;

public class Result
{
    protected internal Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None || !isSuccess && error == Error.None)
            throw new ArgumentException("Invalid error", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => Result<TValue>.Success(value);

    public static Result<TValue> Failure<TValue>(Error error) => Result<TValue>.Failure(error);
}

public class Result<T> : Result
{
    private readonly T? _value;

    protected internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value =>
        IsSuccess
            ? _value!
            : throw new InvalidOperationException(
                "The value of a failure result can not be accessed."
            );

    public static implicit operator Result<T>(T? value) => Create(value);

    public static implicit operator Result<T>(Error error) => Failure(error);

    public static Result<T> Create(T? value)
    {
        return value is not null ? Success(value) : Failure(default);
    }

    public static Result<T> Success(T value) => new(value, true, Error.None);

    public static new Result<T> Failure(Error? error) => new(default, false, error);
}
