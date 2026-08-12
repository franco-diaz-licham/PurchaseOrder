using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Domain.Core;

namespace PurchaseOrderApp.Application.UseCases;

public interface IInventoryItemService
{
    Task<Result<List<InventoryItemResponse>>> ListAsync(CancellationToken cancellationToken);

    Task<Result> ChangeStandardCostAsync(ChangeInventoryItemStandardCostCommand command, CancellationToken cancellationToken);
}

public sealed class InventoryItemService(IInventoryItemRepository inventoryItemRepository, IUnitOfWork unitOfWork) : IInventoryItemService
{
    public async Task<Result<List<InventoryItemResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var inventoryItems = await inventoryItemRepository.ListAsync(cancellationToken);
        var response = inventoryItems
            .Select(item => new InventoryItemResponse(item.Id.Value, item.Sku, item.Name, item.Category.ToString(), item.TrackingMode.ToString(), item.StandardCost.Value))
            .ToList();

        return Result.Success(response);
    }

    public async Task<Result> ChangeStandardCostAsync(ChangeInventoryItemStandardCostCommand command, CancellationToken cancellationToken)
    {
        if (command.InventoryItemId.Value == Guid.Empty) return Result.Fail("Inventory item id is required.", ResultStatus.Invalid);
        if (string.IsNullOrWhiteSpace(command.User)) return Result.Fail("User is required.", ResultStatus.Invalid);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try {
            var inventoryItem = await inventoryItemRepository.GetAsync(command.InventoryItemId, cancellationToken);
            if (inventoryItem is null) {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Fail("Inventory item was not found.", ResultStatus.NotFound);
            }

            inventoryItem.ChangeStandardCost(command.StandardCost, command.User, command.OccurredAt);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success();
        } catch (DomainException ex) {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            return Result.Fail(ex.Message, ResultStatus.Invalid);
        } catch {
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
