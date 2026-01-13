using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Vehicles.Dto;

namespace GestAuto.Stock.Application.Vehicles.Commands;

public sealed record CreateCheckOutCommand(
    Guid VehicleId,
    Guid ResponsibleUserId,
    CheckOutCreateRequest Request) : ICommand<CheckOutResponse>;
