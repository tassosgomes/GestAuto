using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Reservations.Dto;

namespace GestAuto.Stock.Application.Reservations.Commands;

public sealed record ExtendReservationCommand(
    Guid ReservationId,
    Guid ExtendedByUserId,
    ExtendReservationRequest Request) : ICommand<ReservationResponse>;
