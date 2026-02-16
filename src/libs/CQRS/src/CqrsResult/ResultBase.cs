namespace CQRS.CqrsResult;

public abstract class ResultBase
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private readonly List<Error> _errors;
    public IReadOnlyList<Error> Errors => _errors;

    protected ResultBase(bool isSuccess, IEnumerable<Error>? errors)
    {
        IsSuccess = isSuccess;
        _errors = errors?.ToList() ?? [];

        if (IsSuccessWithErrors)
        {
            throw new InvalidOperationException("Successful result cannot contain errors");
        }

        if (IsFailureWithoutErrors)
        {
            throw new InvalidOperationException("Failed result must contain at least one error");
        }
    }

    private bool IsSuccessWithErrors => IsSuccess && _errors.Count > 0;
    private bool IsFailureWithoutErrors => !IsSuccess && _errors.Count == 0;
}
