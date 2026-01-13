using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Application.Vehicles.Dto;

namespace GestAuto.Stock.Application.Vehicles.Commands;

public sealed record CreateVehicleCommand(Guid RequestedByUserId, VehicleCreate Request) : ICommand<VehicleResponse>;
