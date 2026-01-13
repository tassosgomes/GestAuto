using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Vehicles.Dto;

public sealed record CheckOutResponse(
    Guid Id,
    Guid VehicleId,
    CheckOutReason Reason,
    DateTime OccurredAt,
    Guid ResponsibleUserId,
    string? Notes,
    VehicleStatus CurrentStatus);
