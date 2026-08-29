using PurchaseOrderApp.BL.Commands;
using PurchaseOrderApp.BL.Common;
using PurchaseOrderApp.BL.Responses;

namespace PurchaseOrderApp.BL.Workflows;

public interface IReservationService
{
    Task<Result<List<ReservationResponse>>> ListAsync(CancellationToken cancellationToken);

    Task<Result<ReservationResponse>> ReserveAsync(CreateReservationCommand command, CancellationToken cancellationToken);

    Task<Result<ReservationResponse>> ReleaseAsync(ReleaseReservationCommand command, CancellationToken cancellationToken);
}
