using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Reservations.Dto;

public sealed record ReservationListItem(
    Guid Id,
    Guid VehicleId,
    string VehicleMake,
    string VehicleModel,
    string? VehicleTrim,
    int VehicleYearModel,
    string? VehiclePlate,
    ReservationType Type,
    ReservationStatus Status,
    Guid SalesPersonId,
    string SalesPersonName,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc,
    DateTime? BankDeadlineAtUtc
);
