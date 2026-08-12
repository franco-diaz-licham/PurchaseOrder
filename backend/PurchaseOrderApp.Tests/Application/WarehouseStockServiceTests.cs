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
    private WarehouseStockService _sut = default!;

    [SetUp]
    public void SetUp()
    {
        _warehouseStock = new Mock<IWarehouseStockRepository>(MockBehavior.Strict);
        _sut = new WarehouseStockService(_warehouseStock.Object);
    }

    [Test]
    public async Task ListAsync_ShouldReturnWarehouseStockResponses()
    {
        // Arrange
        var item = TestData.CreateUnitItem();
        var response = new WarehouseStockResponse(
            TestData.WarehouseId.Value,
            item.Id.Value,
            35,
            12,
            23);

        _warehouseStock
            .Setup(repo => repo.ListResponsesAsync(TestData.WarehouseId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([response]);

        // Act
        var result = await _sut.ListAsync(TestData.WarehouseId, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Single().ShouldBe(response);
        _warehouseStock.Verify(repo => repo.ListResponsesAsync(TestData.WarehouseId, It.IsAny<CancellationToken>()), Times.Once);
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
    }
}
