using PurchaseOrderApp.BL.Common;

namespace PurchaseOrderApp.BL.Commands;

internal static class CommandValidation
{
    public static Result All(params Result[] results)
    {
        return results.FirstOrDefault(result => !result.IsSuccess) ?? Result.Success();
    }

    public static Result Required(Guid value, string message)
    {
        return value == Guid.Empty ? Result.Fail(message) : Result.Success();
    }

    public static Result User(string user)
    {
        return string.IsNullOrWhiteSpace(user) ? Result.Fail("User is required.") : Result.Success();
    }

    public static Result Quantity(decimal quantity, string zeroMessage)
    {
        if (quantity < 0) return Result.Fail("Quantity cannot be negative.");
        if (decimal.Round(quantity, 3) != quantity) return Result.Fail("Quantity cannot have more than 3 decimal places.");
        if (quantity == 0) return Result.Fail(zeroMessage);

        return Result.Success();
    }
}
