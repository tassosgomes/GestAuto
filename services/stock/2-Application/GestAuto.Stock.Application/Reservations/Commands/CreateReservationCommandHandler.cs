using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Reservations.Dto;
using GestAuto.Stock.Domain.Entities;
using GestAuto.Stock.Domain.Enums;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.Application.Reservations.Commands;

public sealed class CreateReservationCommandHandler : ICommandHandler<CreateReservationCommand, ReservationResponse>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReservationCommandHandler(
        IVehicleRepository vehicleRepository,
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservationResponse> HandleAsync(CreateReservationCommand command, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(command.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            throw new NotFoundException("Vehicle not found.");
        }

        var existingActive = await _reservationRepository.GetActiveByVehicleIdAsync(command.VehicleId, cancellationToken);
        if (existingActive is not null)
        {
            throw new ConflictException("Vehicle already has an active reservation.");
        }

        var request = command.Request;

        var reservation = new Reservation(
            vehicleId: command.VehicleId,
            type: request.Type,
            salesPersonId: command.RequestedByUserId,
            createdAtUtc: DateTime.UtcNow,
            contextType: request.ContextType,
            contextId: request.ContextId,
            bankDeadlineAtUtc: request.BankDeadlineAtUtc);

        // Keep vehicle status consistent, but avoid emitting reservation.* events from Vehicle.
        vehicle.ChangeStatusManually(VehicleStatus.Reserved, command.RequestedByUserId, reason: "reservation-created");

        await _reservationRepository.AddAsync(reservation, cancellationToken);
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return MapToResponse(reservation);
    }

    internal static ReservationResponse MapToResponse(Reservation reservation)
    {
        return new ReservationResponse(
            Id: reservation.Id,
            VehicleId: reservation.VehicleId,
            Type: reservation.Type,
            Status: reservation.Status,
            SalesPersonId: reservation.SalesPersonId,
            ContextType: reservation.ContextType,
            ContextId: reservation.ContextId,
            CreatedAtUtc: reservation.CreatedAtUtc,
            ExpiresAtUtc: reservation.ExpiresAtUtc,
            BankDeadlineAtUtc: reservation.BankDeadlineAtUtc,
            CancelledAtUtc: reservation.CancelledAtUtc,
            CancelledByUserId: reservation.CancelledByUserId,
            CancelReason: reservation.CancelReason,
            ExtendedAtUtc: reservation.ExtendedAtUtc,
            ExtendedByUserId: reservation.ExtendedByUserId,
            PreviousExpiresAtUtc: reservation.PreviousExpiresAtUtc);
    }
}
