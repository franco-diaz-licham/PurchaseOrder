using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows.Reservations;

public sealed class ReservationQueryService(IStockReservationRepository stockReservationRepository)
{
    public async Task<Result<List<ReservationResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return Result.Success(await stockReservationRepository.ListResponsesAsync(cancellationToken));
    }
}
