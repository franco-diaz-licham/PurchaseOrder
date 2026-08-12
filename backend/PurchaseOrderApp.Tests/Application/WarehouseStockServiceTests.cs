using Moq;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Application.Ports;
using PurchaseOrderApp.Application.UseCases;
using PurchaseOrderApp.Domain.ValueObjects;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Application;

[TestFixture]
public sealed class WarehouseStockServiceTests
{
    private Mock<IWarehouseStockRepository> _warehouseStock = default!;
    private Mock<IStockReservationRepository> _stockReservations = default!;
    private WarehouseStockService _sut = default!;

    [SetUp]
    public void SetUp()
    {
        _warehouseStock = new Mock<IWarehouseStockRepository>(MockBehavior.Strict);
        _stockReservations = new Mock<IStockReservationRepository>(MockBehavior.Strict);
        _sut = new WarehouseStockService(_warehouseStock.Object, _stockReservations.Object);
    }

    [Test]
    public async Task ListAsync_ShouldReturnStockWithAvailableQuantity()
    {
        // Arrange
        var item = TestData.CreateUnitItem();
        var stock = TestData.CreateWarehouseStock(TestData.WarehouseId, item.Id, onHandQuantity: 35);

        _warehouseStock
            .Setup(repo => repo.ListAsync(TestData.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([stock]);
        _stockReservations
            .Setup(repo => repo.GetActiveReservedQuantityAsync(TestData.WarehouseId, item.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Quantity(12));

        // Act
        var result = await _sut.ListAsync(TestData.WarehouseId, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var response = result.Value!.Single();
        response.WarehouseId.ShouldBe(TestData.WarehouseId.Value);
        response.InventoryItemId.ShouldBe(item.Id.Value);
        response.OnHandQuantity.ShouldBe(35);
        response.ActiveReservedQuantity.ShouldBe(12);
        response.AvailableQuantity.ShouldBe(23);
        _warehouseStock.Verify(repo => repo.ListAsync(TestData.WarehouseId, It.IsAny<CancellationToken>()), Times.Once);
        _stockReservations.Verify(repo => repo.GetActiveReservedQuantityAsync(TestData.WarehouseId, item.Id, It.IsAny<CancellationToken>()), Times.Once);
        VerifyNoOtherCalls();
    }

    [Test]
    public async Task ListAsync_ShouldCalculateAvailableQuantityForEachStockRow()
    {
        // Arrange
        var unitItem = TestData.CreateUnitItem();
        var weightItem = TestData.CreateWeightItem();
        var unitStock = TestData.CreateWarehouseStock(TestData.WarehouseId, unitItem.Id, onHandQuantity: 40);
        var weightStock = TestData.CreateWarehouseStock(TestData.WarehouseId, weightItem.Id, onHandQuantity: 35.500m);

        _warehouseStock
            .Setup(repo => repo.ListAsync(TestData.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([unitStock, weightStock]);
        _stockReservations
            .Setup(repo => repo.GetActiveReservedQuantityAsync(TestData.WarehouseId, unitItem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Quantity(8));
        _stockReservations
            .Setup(repo => repo.GetActiveReservedQuantityAsync(TestData.WarehouseId, weightItem.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Quantity(10.250m));

        // Act
        var result = await _sut.ListAsync(TestData.WarehouseId, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Count.ShouldBe(2);
        result.Value[0].AvailableQuantity.ShouldBe(32);
        result.Value[1].AvailableQuantity.ShouldBe(25.250m);
        _warehouseStock.Verify(repo => repo.ListAsync(TestData.WarehouseId, It.IsAny<CancellationToken>()), Times.Once);
        _stockReservations.Verify(repo => repo.GetActiveReservedQuantityAsync(TestData.WarehouseId, unitItem.Id, It.IsAny<CancellationToken>()), Times.Once);
        _stockReservations.Verify(repo => repo.GetActiveReservedQuantityAsync(TestData.WarehouseId, weightItem.Id, It.IsAny<CancellationToken>()), Times.Once);
        VerifyNoOtherCalls();
    }

    [Test]
    public async Task ListAsync_ShouldReturnInvalid_WhenWarehouseIdIsEmpty()
    {
        // Arrange
        var warehouseId = new WarehouseId(Guid.Empty);

        // Act
        var result = await _sut.ListAsync(warehouseId, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.Error.ShouldBe("Warehouse id is required.");
        VerifyNoOtherCalls();
    }

    private void VerifyNoOtherCalls()
    {
        _warehouseStock.VerifyNoOtherCalls();
        _stockReservations.VerifyNoOtherCalls();
    }
}
