using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Reservations.Dto;
using GestAuto.Stock.Domain.Exceptions;
using GestAuto.Stock.Domain.Interfaces;

namespace GestAuto.Stock.Application.Reservations.Commands;

public sealed class ExtendReservationCommandHandler : ICommandHandler<ExtendReservationCommand, ReservationResponse>
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ExtendReservationCommandHandler(IReservationRepository reservationRepository, IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservationResponse> HandleAsync(ExtendReservationCommand command, CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetByIdAsync(command.ReservationId, cancellationToken);
        if (reservation is null)
        {
            throw new NotFoundException("Reservation not found.");
        }

        reservation.Extend(command.ExtendedByUserId, command.Request.NewExpiresAtUtc, DateTime.UtcNow);

        await _reservationRepository.UpdateAsync(reservation, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return CreateReservationCommandHandler.MapToResponse(reservation);
    }
}
