namespace CQRS.CqrsResult;

public interface IFailureResult<out TSelf>
    where TSelf : ResultBase
{
    static abstract TSelf Fail(IEnumerable<Error> errors);
}

public sealed class Result : ResultBase, IFailureResult<Result>
{
    private Result(bool isSuccess, IEnumerable<Error> errors)
        : base(isSuccess, errors)
    {
    }

    public static Result Ok()
        => new(true, []);
    public static Result<T> Ok<T>(T value)
        => Result<T>.Ok(value);

    public static Result Fail(Error error)
    => new(false, [error]);

    public static Result Fail(params Error[] errors)
        => new(false, errors);

    public static Result Fail(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new(false, errors);
    }
}


public sealed class Result<T> : ResultBase, IFailureResult<Result<T>>
{
    public T? Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException("Cannot access the value of a failed result.");
            }

            return field;
        }

        private set => field = value;
    }

    public bool TryGetValue(out T? value)
    {
        if (IsSuccess)
        {
            value = Value;
            return true;
        }

        value = default;

        return false;
    }

    public TResult Match<TResult>(
                        Func<T, TResult> onSuccess,
                        Func<IReadOnlyList<Error>, TResult> onFailure)
    {
        return IsSuccess
            ? onSuccess(Value!)
            : onFailure(Errors);
    }

    private Result(bool isSuccess, T? value, IEnumerable<Error>? errors)
        : base(isSuccess, errors) => Value = value;

    public static Result<T> Ok(T value)
        => new(true, value, []);

    /// <summary>
    /// Factory method to create a failed Result with a single error.
    /// </summary>
    /// <param name="error"> see: <see cref="Error"/></param>
    /// <returns>
    /// An instance of <see cref="Result{T}"/> representing a failed operation with the specified error.
    /// </returns>
    public static Result<T> Fail(Error error)
        => new(false, default, [error]);


    /// <summary>
    /// Factory method to create a failed Result with multiple errors.
    /// </summary>
    /// <param name="errors"></param>
    /// see: <see cref="Error"/>
    /// <returns>
    /// An instance of <see cref="Result{T}"/> representing a failed operation with the specified errors.
    /// </returns>
    public static Result<T> Fail(params Error[] errors)
        => new(false, default, errors);

    public static Result<T> Fail(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        return new(false, default, errors);
    }
}
