using Moq;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Application.UseCases;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Application;

[TestFixture]
public sealed class InventoryItemServiceTests
{
    private Mock<IInventoryItemRepository> _inventoryItems = default!;
    private Mock<IUnitOfWork> _unitOfWork = default!;
    private InventoryItemService _sut = default!;

    [SetUp]
    public void SetUp()
    {
        _inventoryItems = new Mock<IInventoryItemRepository>(MockBehavior.Strict);
        _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _sut = new InventoryItemService(_inventoryItems.Object, _unitOfWork.Object);
    }

    [Test]
    public async Task ListAsync_ShouldReturnInventoryItemResponses()
    {
        // Arrange
        var item = TestData.CreateUnitItem();
        var expectedResponse = new InventoryItemResponse(
            item.Id.Value,
            item.Sku,
            item.Name,
            item.Category.ToString(),
            item.TrackingMode.ToString(),
            item.StandardCost.Value);
        _inventoryItems
            .Setup(repo => repo.ListResponsesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([expectedResponse]);

        // Act
        var result = await _sut.ListAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var response = result.Value!.Single();
        response.InventoryItemId.ShouldBe(item.Id.Value);
        response.Sku.ShouldBe(item.Sku);
        _inventoryItems.Verify(repo => repo.ListResponsesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ChangeStandardCostAsync_ShouldUpdateCostAndCommit()
    {
        // Arrange
        var item = TestData.CreateUnitItem();
        var command = new ChangeInventoryItemStandardCostCommand(item.Id, new Money(2.50m), TestData.User, TestData.OccurredAt);

        _unitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _unitOfWork.Setup(uow => uow.CommitTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _inventoryItems
            .Setup(repo => repo.GetAsync(command.InventoryItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        // Act
        var result = await _sut.ChangeStandardCostAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        item.StandardCost.Value.ShouldBe(2.50m);
        _unitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(uow => uow.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ChangeStandardCostAsync_ShouldRollbackAndReturnNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var command = new ChangeInventoryItemStandardCostCommand(new InventoryItemId(Guid.NewGuid()), new Money(2.50m), TestData.User, TestData.OccurredAt);

        _unitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(uow => uow.RollbackTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _inventoryItems
            .Setup(repo => repo.GetAsync(command.InventoryItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((InventoryItem?)null);

        // Act
        var result = await _sut.ChangeStandardCostAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
        result.Error.ShouldBe("Inventory item was not found.");
        _unitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(uow => uow.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
