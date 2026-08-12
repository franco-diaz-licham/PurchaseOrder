namespace PurchaseOrderApp.Domain.Services;

/// <summary>
/// Creates purchase order numbers using the domain's PO number format.
/// </summary>
public static class PurchaseOrderNumberGenerator
{
    public static string Create()
    {
        return $"PO-{Random.Shared.Next(10000, 100000)}";
    }
}
