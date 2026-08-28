namespace PurchaseOrderApp.Api.Controllers.Models;

public sealed record ChangeInventoryItemStandardCostRequest(
    decimal StandardCost,
    string User);
