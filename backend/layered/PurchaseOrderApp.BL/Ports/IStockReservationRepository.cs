using PurchaseOrderApp.BL.Models;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Ports;

public interface IStockReservationRepository
{
    Task<StockReservation?> GetAsync(Guid stockReservationId, CancellationToken cancellationToken);

    Task<List<ReservationResponse>> ListResponsesAsync(CancellationToken cancellationToken);

    Task<decimal> GetActiveReservedQuantityAsync(Guid warehouseId, Guid inventoryItemId, CancellationToken cancellationToken);

    Task<List<StockReservation>> ListActiveByLineAsync(Guid purchaseOrderLineId, CancellationToken cancellationToken);

    Task AddAsync(StockReservation stockReservation, CancellationToken cancellationToken);
}
