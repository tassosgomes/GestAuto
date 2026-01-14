using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Reservations.Dto;

public sealed record CreateReservationRequest(
    ReservationType Type,
    string ContextType,
    Guid? ContextId,
    DateTime? BankDeadlineAtUtc);
