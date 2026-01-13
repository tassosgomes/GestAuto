using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Vehicles.Dto;

namespace GestAuto.Stock.Application.Vehicles.Commands;

public sealed record CreateCheckInCommand(
    Guid VehicleId,
    Guid ResponsibleUserId,
    CheckInCreateRequest Request) : ICommand<CheckInResponse>;
