namespace PurchaseOrderApp.Api.Models;

public sealed record ApiResponse(int StatusCode, string Message);

public sealed record ApiResponse<T>(int StatusCode, string Message, T Data);
