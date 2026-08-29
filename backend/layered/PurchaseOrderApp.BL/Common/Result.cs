namespace PurchaseOrderApp.BL.Common;

public enum ResultStatus
{
    Success = 1,
    Created = 2,
    Invalid = 3,
    NotFound = 4
}

public class Result
{
    protected Result(bool isSuccess, ResultStatus status, string? error)
    {
        IsSuccess = isSuccess;
        Status = status;
        Error = error;
    }

    public bool IsSuccess { get; }

    public ResultStatus Status { get; }

    public string? Error { get; }

    public static Result Success() => new(true, ResultStatus.Success, null);

    public static Result Fail(string error, ResultStatus status = ResultStatus.Invalid) => new(false, status, error);

    public static Result<T> Success<T>(T value) => new(true, ResultStatus.Success, null, value);

    public static Result<T> Created<T>(T value) => new(true, ResultStatus.Created, null, value);

    public static Result<T> Fail<T>(string error, ResultStatus status = ResultStatus.Invalid) => new(false, status, error, default);
}

public sealed class Result<T> : Result
{
    internal Result(bool isSuccess, ResultStatus status, string? error, T? value) : base(isSuccess, status, error)
    {
        Value = value;
    }

    public T? Value { get; }
}
