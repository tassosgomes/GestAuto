using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Vehicles.Dto;

public sealed record ChangeVehicleStatusRequest(VehicleStatus NewStatus, string Reason);
