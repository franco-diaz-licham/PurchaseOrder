using Moq;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Application.UseCases;
using PurchaseOrderApp.Domain.Entities;
using PurchaseOrderApp.Domain.Services;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Application;

[TestFixture]
public sealed class ReservationServiceTests
{
    private Mock<IPurchaseOrderRepository> _purchaseOrders = default!;
    private Mock<IInventoryItemRepository> _inventoryItems = default!;
    private Mock<IWarehouseStockRepository> _warehouseStock = default!;
    private Mock<IStockReservationRepository> _stockReservations = default!;
    private Mock<IUnitOfWork> _unitOfWork = default!;
    private ReservationService _sut = default!;

    [SetUp]
    public void SetUp()
    {
        _purchaseOrders = new Mock<IPurchaseOrderRepository>(MockBehavior.Strict);
        _inventoryItems = new Mock<IInventoryItemRepository>(MockBehavior.Strict);
        _warehouseStock = new Mock<IWarehouseStockRepository>(MockBehavior.Strict);
        _stockReservations = new Mock<IStockReservationRepository>(MockBehavior.Strict);
        _unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

        _sut = new ReservationService(
            _purchaseOrders.Object,
            _inventoryItems.Object,
            _warehouseStock.Object,
            _stockReservations.Object,
            _unitOfWork.Object);
    }

    [Test]
    public async Task ReserveAsync_ShouldCreateReservationAndCommit()
    {
        // Arrange
        var purchaseOrder = CreateApprovedPurchaseOrder(out var line);
        var item = line.InventoryItem;
        var stock = TestData.CreateWarehouseStock(purchaseOrder.WarehouseId, item.Id, onHandQuantity: 100);
        var command = new CreateReservationCommand(line.Id, purchaseOrder.WarehouseId, new Quantity(4), TestData.User, TestData.OccurredAt);
        StockReservation? addedReservation = null;

        SetupSuccessfulTransaction();
        _purchaseOrders
            .Setup(repo => repo.GetByLineIdAsync(command.PurchaseOrderLineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);
        _inventoryItems
            .Setup(repo => repo.GetAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _warehouseStock
            .Setup(repo => repo.GetForUpdateAsync(command.WarehouseId, item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);
        _stockReservations
            .Setup(repo => repo.GetActiveReservedQuantityAsync(command.WarehouseId, item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Quantity.Zero);
        _stockReservations
            .Setup(repo => repo.AddAsync(It.IsAny<StockReservation>(), It.IsAny<CancellationToken>()))
            .Callback<StockReservation, CancellationToken>((reservation, _) => addedReservation = reservation)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ReserveAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Created);
        result.Value!.QuantityReserved.ShouldBe(4);
        addedReservation.ShouldNotBeNull();
        addedReservation.QuantityReserved.Value.ShouldBe(4);
        line.QuantityReserved.Value.ShouldBe(4);
        VerifySuccessfulTransaction();
    }

    [Test]
    public async Task ReserveAsync_ShouldRollbackAndReturnNotFound_WhenPurchaseOrderLineDoesNotExist()
    {
        // Arrange
        var command = new CreateReservationCommand(
            new PurchaseOrderLineId(Guid.NewGuid()),
            TestData.WarehouseId,
            new Quantity(1),
            TestData.User,
            TestData.OccurredAt);

        _unitOfWork.Setup(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _unitOfWork.Setup(uow => uow.RollbackTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _purchaseOrders
            .Setup(repo => repo.GetByLineIdAsync(command.PurchaseOrderLineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseOrder?)null);

        // Act
        var result = await _sut.ReserveAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.NotFound);
        result.Error.ShouldBe("Purchase order line was not found.");
        _unitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(uow => uow.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ReleaseAsync_ShouldReleaseReservationAndCommit()
    {
        // Arrange
        var purchaseOrder = CreateApprovedPurchaseOrder(out var line);
        var item = line.InventoryItem;
        var stock = TestData.CreateWarehouseStock(purchaseOrder.WarehouseId, item.Id, onHandQuantity: 100);
        var reservation = StockReservationDomainService.Reserve(
            purchaseOrder,
            line.Id,
            stock,
            item,
            Quantity.Zero,
            new Quantity(6),
            TestData.User,
            TestData.OccurredAt);
        var command = new ReleaseReservationCommand(reservation.Id, new Quantity(2), TestData.User, TestData.OccurredAt);

        SetupSuccessfulTransaction();
        _stockReservations
            .Setup(repo => repo.GetAsync(command.StockReservationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _inventoryItems
            .Setup(repo => repo.GetAsync(item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _purchaseOrders
            .Setup(repo => repo.GetByLineIdAsync(line.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(purchaseOrder);
        _warehouseStock
            .Setup(repo => repo.GetForUpdateAsync(reservation.WarehouseId, reservation.InventoryItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(stock);
        _stockReservations
            .Setup(repo => repo.GetActiveReservedQuantityAsync(reservation.WarehouseId, reservation.InventoryItemId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Quantity(6));

        // Act
        var result = await _sut.ReleaseAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.QuantityReserved.ShouldBe(4);
        reservation.QuantityReserved.Value.ShouldBe(4);
        line.QuantityReserved.Value.ShouldBe(4);
        VerifySuccessfulTransaction();
    }

    [Test]
    public async Task ReserveAsync_ShouldReturnInvalidWithoutTransaction_WhenCommandIsInvalid()
    {
        // Arrange
        var command = new CreateReservationCommand(
            new PurchaseOrderLineId(Guid.Empty),
            TestData.WarehouseId,
            new Quantity(1),
            TestData.User,
            TestData.OccurredAt);

        // Act
        var result = await _sut.ReserveAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.Error.ShouldBe("Purchase order line id is required.");
    }

    private static PurchaseOrder CreateApprovedPurchaseOrder(out PurchaseOrderLine line)
    {
        var purchaseOrder = TestData.CreatePendingPurchaseOrder();
        var item = TestData.CreateUnitItem();
        line = purchaseOrder.AddLine(item, new Quantity(10), TestData.User, TestData.OccurredAt);
        purchaseOrder.Approve(TestData.User, TestData.OccurredAt);
        return purchaseOrder;
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
        _inventoryItems.VerifyNoOtherCalls();
        _warehouseStock.VerifyNoOtherCalls();
        _stockReservations.VerifyNoOtherCalls();
        _unitOfWork.VerifyNoOtherCalls();
    }
}
