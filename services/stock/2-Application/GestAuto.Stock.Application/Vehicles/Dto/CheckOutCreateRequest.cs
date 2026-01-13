using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Vehicles.Dto;

public sealed record CheckOutCreateRequest(
    CheckOutReason Reason,
    DateTime? OccurredAt,
    string? Notes);
