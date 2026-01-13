using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Vehicles.Dto;

public sealed record CheckInResponse(
    Guid Id,
    Guid VehicleId,
    CheckInSource Source,
    DateTime OccurredAt,
    Guid ResponsibleUserId,
    string? Notes,
    VehicleStatus CurrentStatus,
    Guid? CurrentOwnerUserId);
