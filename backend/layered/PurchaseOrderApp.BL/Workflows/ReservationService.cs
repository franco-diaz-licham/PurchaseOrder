using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Ports;
using PurchaseOrderApp.BL.Responses;
using PurchaseOrderApp.BL.Workflows.Reservations;

namespace PurchaseOrderApp.BL.Workflows;

public sealed class ReservationService(
    IStockReservationRepository stockReservationRepository,
    ReserveStockWorkflow reserveStock,
    ReleaseReservationWorkflow releaseReservation) : IReservationService
{
    public async Task<Result<List<ReservationResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        return Result.Success(await stockReservationRepository.ListResponsesAsync(cancellationToken));
    }

    public Task<Result<ReservationResponse>> ReserveAsync(CreateReservationCommand command, CancellationToken cancellationToken) =>
        reserveStock.ExecuteAsync(command, cancellationToken);

    public Task<Result<ReservationResponse>> ReleaseAsync(ReleaseReservationCommand command, CancellationToken cancellationToken) =>
        releaseReservation.ExecuteAsync(command, cancellationToken);
}
