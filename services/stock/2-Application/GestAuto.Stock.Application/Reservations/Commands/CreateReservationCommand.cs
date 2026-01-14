using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Reservations.Dto;

namespace GestAuto.Stock.Application.Reservations.Commands;

public sealed record CreateReservationCommand(
    Guid VehicleId,
    Guid RequestedByUserId,
    CreateReservationRequest Request) : ICommand<ReservationResponse>;
