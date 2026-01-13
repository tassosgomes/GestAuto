using GestAuto.Stock.Application.Interfaces;
using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Vehicles.Commands;

public sealed record ChangeVehicleStatusCommand(
    Guid VehicleId,
    VehicleStatus NewStatus,
    string Reason,
    Guid ChangedByUserId) : ICommand<bool>;
