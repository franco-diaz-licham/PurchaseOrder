using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using PurchaseOrderApp.Application.Models;
using PurchaseOrderApp.Domain.Enums;
using PurchaseOrderApp.Tests.Shared;
using Shouldly;

namespace PurchaseOrderApp.Tests.Api;

public sealed class PurchaseOrderEndpointTests : ApiEndpointTestFixture
{
    [Test]
    public async Task CreatePurchaseOrder_ShouldCreatePendingPurchaseOrderWithServerNumber()
    {
        var seed = await PurchaseOrderScenarioSeeder.SeedApprovedLineAsync(Db, quantityOrdered: 5m);

        var response = await Client.PostAsJsonAsync("/api/purchase-order", new {
            WarehouseId = seed.WarehouseId.Value,
            Lines = Array.Empty<object>(),
            User = TestData.User
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var purchaseOrder = await ReadDataAsync<PurchaseOrderResponse>(response);

        purchaseOrder.PurchaseOrderNumber.ShouldStartWith("PO-");
        purchaseOrder.PurchaseOrderNumber.Length.ShouldBeLessThanOrEqualTo(8);
        purchaseOrder.Status.ShouldBe(nameof(PurchaseOrderStatus.Pending));
        purchaseOrder.WarehouseId.ShouldBe(seed.WarehouseId.Value);
        purchaseOrder.Lines.ShouldBeEmpty();
    }

    [Test]
    public async Task AddLine_ShouldRejectDuplicateInventoryItem()
    {
        var seed = await PurchaseOrderScenarioSeeder.SeedApprovedLineAsync(Db);
        var purchaseOrder = await CreatePurchaseOrderAsync(seed.WarehouseId.Value);

        var request = new {
            InventoryItemId = seed.InventoryItemId.Value,
            QuantityOrdered = 5m,
            User = TestData.User
        };

        var firstResponse = await Client.PostAsJsonAsync($"/api/purchase-order/{purchaseOrder.PurchaseOrderId}/lines", request);
        var secondResponse = await Client.PostAsJsonAsync($"/api/purchase-order/{purchaseOrder.PurchaseOrderId}/lines", request);

        firstResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        secondResponse.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ReserveAndRelease_ShouldUpdateFinanceAndAuditLog()
    {
        var seed = await PurchaseOrderScenarioSeeder.SeedApprovedLineAsync(
            Db,
            trackingMode: InventoryTrackingMode.Weight,
            standardCost: 1.75m,
            onHandQuantity: 35m,
            quantityOrdered: 25m);

        var reserveResponse = await Client.PostAsJsonAsync("/api/reservation", new {
            PurchaseOrderLineId = seed.PurchaseOrderLineId.Value,
            WarehouseId = seed.WarehouseId.Value,
            Quantity = 10.500m,
            User = TestData.User
        });

        reserveResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
        var reservation = await ReadDataAsync<ReservationResponse>(reserveResponse);

        var releaseResponse = await Client.PostAsJsonAsync($"/api/reservation/{reservation.StockReservationId}/release", new {
            Quantity = 4.250m,
            User = TestData.User
        });

        releaseResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var releasedReservation = await ReadDataAsync<ReservationResponse>(releaseResponse);
        releasedReservation.QuantityReserved.ShouldBe(6.250m);

        var financeResponse = await Client.GetAsync("/api/finance/warehouse-committed-values");
        financeResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var financeValues = await ReadDataAsync<List<WarehouseCommittedStockValueResponse>>(financeResponse);
        var warehouseValue = financeValues.Single(value => value.WarehouseId == seed.WarehouseId.Value);
        warehouseValue.ReservedQuantity.ShouldBe(6.250m);
        warehouseValue.CommittedValue.ShouldBe(10.9375m);
        warehouseValue.Reservations.Single().UnitCostSnapshot.ShouldBe(1.75m);

        var auditResponse = await Client.GetAsync("/api/audit-log");
        auditResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        var auditLog = await ReadDataAsync<List<AuditLogResponse>>(auditResponse);
        auditLog.Select(entry => entry.Quantity).ShouldContain(10.500m);
        auditLog.Select(entry => entry.Quantity).ShouldContain(4.250m);
    }

    [Test]
    public async Task ReserveAsync_ShouldAllowOnlyOneRequest_WhenTwoRequestsCompeteForTheSameWarehouseStock()
    {
        var seed = await PurchaseOrderScenarioSeeder.SeedCompetingReservationScenarioAsync(Db, onHandQuantity: 10m, quantityOrdered: 7m);

        var firstRequest = ReserveAsync(seed.FirstPurchaseOrderLineId.Value, seed.WarehouseId.Value, 7m);
        var secondRequest = ReserveAsync(seed.SecondPurchaseOrderLineId.Value, seed.WarehouseId.Value, 7m);
        var responses = await Task.WhenAll(firstRequest, secondRequest);

        responses.Count(response => response.StatusCode == HttpStatusCode.Created).ShouldBe(1);
        responses.Count(response => response.StatusCode == HttpStatusCode.BadRequest).ShouldBe(1);

        Db.ChangeTracker.Clear();
        var activeReservations = await Db.StockReservations
            .Where(reservation => reservation.WarehouseId == seed.WarehouseId && reservation.InventoryItemId == seed.InventoryItemId)
            .ToListAsync();

        activeReservations.Count.ShouldBe(1);
        activeReservations.Sum(reservation => reservation.QuantityReserved.Value).ShouldBe(7m);
    }

    private Task<HttpResponseMessage> ReserveAsync(Guid purchaseOrderLineId, Guid warehouseId, decimal quantity)
    {
        return Client.PostAsJsonAsync("/api/reservation", new {
            PurchaseOrderLineId = purchaseOrderLineId,
            WarehouseId = warehouseId,
            Quantity = quantity,
            User = TestData.User
        });
    }

    private async Task<PurchaseOrderResponse> CreatePurchaseOrderAsync(Guid warehouseId)
    {
        var response = await Client.PostAsJsonAsync("/api/purchase-order", new {
            WarehouseId = warehouseId,
            Lines = Array.Empty<object>(),
            User = TestData.User
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return await ReadDataAsync<PurchaseOrderResponse>(response);
    }

}
