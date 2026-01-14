using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Reservations.Dto;

namespace GestAuto.Stock.Application.Reservations.Commands;

public sealed record CancelReservationCommand(
    Guid ReservationId,
    Guid CancelledByUserId,
    bool CanCancelOthers,
    CancelReservationRequest Request) : ICommand<ReservationResponse>;
