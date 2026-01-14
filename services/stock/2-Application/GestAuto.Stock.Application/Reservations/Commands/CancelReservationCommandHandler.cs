using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Reservations.Dto;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.Application.Reservations.Commands;

public sealed class CancelReservationCommandHandler : ICommandHandler<CancelReservationCommand, ReservationResponse>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelReservationCommandHandler(
        IReservationRepository reservationRepository,
        IVehicleRepository vehicleRepository,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservationResponse> HandleAsync(CancelReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);
        if (reservation is null)
        {
            throw new NotFoundException("Reservation not found.");
        }

        if (!command.CanCancelOthers && reservation.SalesPersonId != command.CancelledByUserId)
        {
            throw new ForbiddenException("Sem permissão para cancelar reserva de outro vendedor.");
        }

        var vehicle = await _vehicleRepository.GetByIdAsync(reservation.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new NotFoundException("Vehicle not found.");
        }

        reservation.Cancel(command.CancelledByUserId, command.Request.Reason, DateTime.UtcNow);

        if (vehicle.CurrentStatus == VehicleStatus.Reserved)
        {
            vehicle.ChangeStatusManually(VehicleStatus.InStock, command.CancelledByUserId, reason: "reservation-cancelled");
        }

        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return CreateReservationCommandHandler.MapToResponse(reservation);
    }
}
