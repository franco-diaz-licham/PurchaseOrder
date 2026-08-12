using Moq;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Application.UseCases;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Application;

[TestFixture]
public sealed class PurchaseOrderServiceTests
{
    private Mock<IPurchaseOrderRepository> _purchaseOrders = default!;
    private Mock<IWarehouseRepository> _warehouses = default!;
    private Mock<IInventoryItemRepository> _inventoryItems = default!;
    private Mock<IWarehouseStockRepository> _warehouseStock = default!;
    private Mock<IStockReservationRepository> _stockReservations = default!;
    private Mock<IUnitOfWork> _unitOfWork = default!;
    private PurchaseOrderService _sut = default!;

    [SetUp]
    public void SetUp()
    {
        _purchaseOrders = new Mock<IPurchaseOrderRepository>(MockBehavior.Strict);
        _warehouses = new Mock<IWarehouseRepository>(MockBehavior.Strict);
        _inventoryItems = new Mock<IInventoryItemRepository>(MockBehavior.Strict);
        _warehouseStock = new Mock<IWarehouseStockRepository>(MockBehavior.Strict);
        _stockReservations = new Mock<IStockReservationRepository>(MockBehavior.Strict);
        _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

        _sut = new PurchaseOrderService(
            _purchaseOrders.Object,
            _warehouses.Object,
            _inventoryItems.Object,
            _warehouseStock.Object,
            _stockReservations.Object,
            _unitOfWork.Object);
    }

    [Test]
    public async Task SubmitAsync_ShouldCreatePendingPurchaseOrderAndCommit()
    {
        // Arrange
        var warehouse = Warehouse.Create("SYD", "Sydney Fulfilment Centre", TestData.User, TestData.OccurredAt);
        var item = TestData.CreateUnitItem();
        var command = new SubmitPurchaseOrderCommand(
            warehouse.Id,
            [new SubmitPurchaseOrderLineCommand(item.Id, new Quantity(5))],
            TestData.User,
            TestData.OccurredAt);
        PurchaseOrder? addedPurchaseOrder = null;

        SetupSuccessfulTransaction();
        _warehouses
            .Setup(repo => repo.GetAsync(command.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(warehouse);
        _inventoryItems
            .Setup(repo => repo.GetAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _purchaseOrders
            .Setup(repo => repo.AddAsync(It.IsAny<PurchaseOrder>(), It.IsAny<CancellationToken>()))
            .Callback<PurchaseOrder, CancellationToken>((purchaseOrder, _) => addedPurchaseOrder = purchaseOrder)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.SubmitAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Created);
        result.Value!.Status.ShouldBe(PurchaseOrderStatus.Pending.ToString());
        result.Value.Lines.Single().QuantityOrdered.ShouldBe(5);
        addedPurchaseOrder.ShouldNotBeNull();
        addedPurchaseOrder.Status.ShouldBe(PurchaseOrderStatus.Pending);
        VerifySuccessfulTransaction();
        _purchaseOrders.Verify(repo => repo.AddAsync(It.IsAny<PurchaseOrder>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task SubmitAsync_ShouldRollbackAndReturnNotFound_WhenWarehouseDoesNotExist()
    {
        // Arrange
        var command = new SubmitPurchaseOrderCommand(TestData.WarehouseId, [], TestData.User, TestData.OccurredAt);

        _unitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(uow => uow.RollbackTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _warehouses
            .Setup(repo => repo.GetAsync(command.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Warehouse?)null);

        // Act
        var result = await _sut.SubmitAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
        result.Error.ShouldBe("Warehouse was not found.");
        _unitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(uow => uow.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task AddLineAsync_ShouldAddLineAndCommit()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();
        var command = new AddPurchaseOrderLineCommand(
            purchaseOrder.Id,
            item.Id,
            new Quantity(3),
            TestData.User,
            TestData.OccurredAt);

        SetupSuccessfulTransaction();
        _purchaseOrders
            .Setup(repo => repo.GetAsync(command.PurchaseOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);
        _inventoryItems
            .Setup(repo => repo.GetAsync(command.InventoryItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);

        // Act
        var result = await _sut.AddLineAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Lines.Single().InventoryItemId.ShouldBe(item.Id.Value);
        purchaseOrder.Lines.Single().QuantityOrdered.Value.ShouldBe(3);
        VerifySuccessfulTransaction();
    }

    [Test]
    public async Task ApproveAsync_ShouldApprovePurchaseOrderAndCommit()
    {
        // Arrange
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var command = new ChangePurchaseOrderStatusCommand(purchaseOrder.Id, TestData.User, TestData.OccurredAt);

        SetupSuccessfulTransaction();
        _purchaseOrders
            .Setup(repo => repo.GetAsync(command.PurchaseOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);

        // Act
        var result = await _sut.ApproveAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Status.ShouldBe(PurchaseOrderStatus.Approved.ToString());
        purchaseOrder.Status.ShouldBe(PurchaseOrderStatus.Approved);
        VerifySuccessfulTransaction();
    }

    [Test]
    public async Task AddLineAsync_ShouldReturnInvalidWithoutTransaction_WhenCommandIsInvalid()
    {
        // Arrange
        var command = new AddPurchaseOrderLineCommand(
            new PurchaseOrderId(Guid.Empty),
            new InventoryItemId(Guid.NewGuid()),
            new Quantity(1),
            TestData.User,
            TestData.OccurredAt);

        // Act
        var result = await _sut.AddLineAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.Error.ShouldBe("Purchase order id is required.");
    }

    private void SetupSuccessfulTransaction()
    {
        _unitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _unitOfWork.Setup(uow => uow.CommitTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    private void VerifySuccessfulTransaction()
    {
        _unitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(uow => uow.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private void VerifyNoOtherCalls()
    {
        _purchaseOrders.VerifyNoOtherCalls();
        _warehouses.VerifyNoOtherCalls();
        _inventoryItems.VerifyNoOtherCalls();
        _warehouseStock.VerifyNoOtherCalls();
        _stockReservations.VerifyNoOtherCalls();
        _unitOfWork.VerifyNoOtherCalls();
    }
}
