namespace PurchaseOrderApp.Application.Models;

/// <summary>
/// Describes the application result status returned by a use case.
/// </summary>
public enum ResultStatus
{
    /// <summary>
    /// The request completed successfully.
    /// </summary>
    Success = 1,

    /// <summary>
    /// The request created a new resource.
    /// </summary>
    Created = 2,

    /// <summary>
    /// The request failed validation or a domain rule.
    /// </summary>
    Invalid = 3,

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound = 4
}

/// <summary>
/// Represents the outcome of an application use case without a return value.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, ResultStatus status, string? error)
    {
        IsSuccess = isSuccess;
        Status = status;
        Error = error;
    }

    /// <summary>
    /// Indicates whether the use case completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Status used by the API layer to choose the HTTP response.
    /// </summary>
    public ResultStatus Status { get; }

    /// <summary>
    /// Error message returned when the use case fails.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Creates a successful result without a return value.
    /// </summary>
    public static Result Success() => new(true, ResultStatus.Success, null);

    /// <summary>
    /// Creates a failed result without a return value.
    /// </summary>
    public static Result Fail(string error, ResultStatus status) => new(false, status, error);

    /// <summary>
    /// Creates a successful result with a return value.
    /// </summary>
    public static Result<T> Success<T>(T value) => new(true, ResultStatus.Success, null, value);

    /// <summary>
    /// Creates a result for a newly created resource.
    /// </summary>
    public static Result<T> Created<T>(T value) => new(true, ResultStatus.Created, null, value);

    /// <summary>
    /// Creates a failed result with a typed return shape.
    /// </summary>
    public static Result<T> Fail<T>(string error, ResultStatus status) => new(false, status, error, default);
}

/// <summary>
/// Represents the outcome of an application use case with a return value.
/// </summary>
public sealed class Result<T> : Result
{
    internal Result(bool isSuccess, ResultStatus status, string? error, T? value) : base(isSuccess, status, error)
    {
        Value = value;
    }

    /// <summary>
    /// Value returned by the use case when it succeeds.
    /// </summary>
    public T? Value { get; }
}
