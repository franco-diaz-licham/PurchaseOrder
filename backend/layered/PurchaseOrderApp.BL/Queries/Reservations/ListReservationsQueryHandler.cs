using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Queries.Reservations;

public sealed record ListReservationsQuery;

public sealed class ListReservationsQueryHandler(IStockReservationRepository stockReservationRepository)
{
    public async Task<Result<List<ReservationResponse>>> ExecuteAsync(ListReservationsQuery query, CancellationToken cancellationToken)
    {
        return Result.Success(await stockReservationRepository.ListResponsesAsync(cancellationToken));
    }
}
