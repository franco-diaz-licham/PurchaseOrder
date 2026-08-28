namespace PurchaseOrderApp.BL.Common;

public enum ResultStatus
{
    Success,
    Created,
    Invalid,
    NotFound
}

public sealed record Result(ResultStatus Status, string? Error = null)
{
    public bool IsSuccess => Status is ResultStatus.Success or ResultStatus.Created;

    public static Result Success() => new(ResultStatus.Success);

    public static Result Fail(string error, ResultStatus status = ResultStatus.Invalid) => new(status, error);
}
