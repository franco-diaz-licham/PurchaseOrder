using FluentValidation;
using PurchaseOrderApp.Services.Models;

namespace PurchaseOrderApp.Services.Validation;

public sealed class SubmitPurchaseOrderRequestValidator : AbstractValidator<SubmitPurchaseOrderRequest>
{
    public SubmitPurchaseOrderRequestValidator()
    {
        RuleFor(request => request.WarehouseId)
            .NotEmpty()
            .WithMessage("Warehouse id is required.");

        RuleFor(request => request.User)
            .NotEmpty()
            .WithMessage("User is required.");

        RuleForEach(request => request.Lines)
            .SetValidator(new SubmitPurchaseOrderLineRequestValidator())
            .When(request => request.Lines is not null);
    }
}

public sealed class SubmitPurchaseOrderLineRequestValidator : AbstractValidator<SubmitPurchaseOrderLineRequest>
{
    public SubmitPurchaseOrderLineRequestValidator()
    {
        RuleFor(request => request.InventoryItemId)
            .NotEmpty()
            .WithMessage("Inventory item id is required.");

        RuleFor(request => request.QuantityOrdered)
            .GreaterThan(0)
            .WithMessage("Line quantity must be greater than zero.")
            .PrecisionScale(18, 3, false)
            .WithMessage("Quantity cannot have more than 3 decimal places.");
    }
}

public sealed class AddPurchaseOrderLineRequestValidator : AbstractValidator<AddPurchaseOrderLineRequest>
{
    public AddPurchaseOrderLineRequestValidator()
    {
        RuleFor(request => request.InventoryItemId)
            .NotEmpty()
            .WithMessage("Inventory item id is required.");

        RuleFor(request => request.QuantityOrdered)
            .GreaterThan(0)
            .WithMessage("Line quantity must be greater than zero.")
            .PrecisionScale(18, 3, false)
            .WithMessage("Quantity cannot have more than 3 decimal places.");

        RuleFor(request => request.User)
            .NotEmpty()
            .WithMessage("User is required.");
    }
}

public sealed class RemovePurchaseOrderLineRequestValidator : AbstractValidator<RemovePurchaseOrderLineRequest>
{
    public RemovePurchaseOrderLineRequestValidator()
    {
        RuleFor(request => request.User)
            .NotEmpty()
            .WithMessage("User is required.");
    }
}

public sealed class UpdatePurchaseOrderLineRequestValidator : AbstractValidator<UpdatePurchaseOrderLineRequest>
{
    public UpdatePurchaseOrderLineRequestValidator()
    {
        RuleFor(request => request.QuantityOrdered)
            .GreaterThan(0)
            .WithMessage("Line quantity must be greater than zero.")
            .PrecisionScale(18, 3, false)
            .WithMessage("Quantity cannot have more than 3 decimal places.");

        RuleFor(request => request.User)
            .NotEmpty()
            .WithMessage("User is required.");
    }
}

public sealed class ChangePurchaseOrderStatusRequestValidator : AbstractValidator<ChangePurchaseOrderStatusRequest>
{
    public ChangePurchaseOrderStatusRequestValidator()
    {
        RuleFor(request => request.User)
            .NotEmpty()
            .WithMessage("User is required.");
    }
}

public sealed class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator()
    {
        RuleFor(request => request.PurchaseOrderLineId)
            .NotEmpty()
            .WithMessage("Purchase order line id is required.");

        RuleFor(request => request.WarehouseId)
            .NotEmpty()
            .WithMessage("Warehouse id is required.");

        RuleFor(request => request.Quantity)
            .GreaterThan(0)
            .WithMessage("Reservation quantity must be greater than zero.")
            .PrecisionScale(18, 3, false)
            .WithMessage("Quantity cannot have more than 3 decimal places.");

        RuleFor(request => request.User)
            .NotEmpty()
            .WithMessage("User is required.");
    }
}

public sealed class ReleaseReservationRequestValidator : AbstractValidator<ReleaseReservationRequest>
{
    public ReleaseReservationRequestValidator()
    {
        RuleFor(request => request.Quantity)
            .GreaterThan(0)
            .WithMessage("Release quantity must be greater than zero.")
            .PrecisionScale(18, 3, false)
            .WithMessage("Quantity cannot have more than 3 decimal places.");

        RuleFor(request => request.User)
            .NotEmpty()
            .WithMessage("User is required.");
    }
}

public sealed class ChangeInventoryItemStandardCostRequestValidator : AbstractValidator<ChangeInventoryItemStandardCostRequest>
{
    public ChangeInventoryItemStandardCostRequestValidator()
    {
        RuleFor(request => request.StandardCost)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Money cannot be negative.");

        RuleFor(request => request.User)
            .NotEmpty()
            .WithMessage("User is required.");
    }
}
