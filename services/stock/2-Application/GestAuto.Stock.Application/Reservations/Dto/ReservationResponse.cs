using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Reservations.Dto;

public sealed record ReservationResponse(
    Guid Id,
    Guid VehicleId,
    ReservationType Type,
    ReservationStatus Status,
    Guid SalesPersonId,
    string ContextType,
    Guid? ContextId,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc,
    DateTime? BankDeadlineAtUtc,
    DateTime? CancelledAtUtc,
    Guid? CancelledByUserId,
    string? CancelReason,
    DateTime? ExtendedAtUtc,
    Guid? ExtendedByUserId,
    DateTime? PreviousExpiresAtUtc);
