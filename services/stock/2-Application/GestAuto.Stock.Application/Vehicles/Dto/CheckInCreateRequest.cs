using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Vehicles.Dto;

public sealed record CheckInCreateRequest(
    CheckInSource Source,
    DateTime? OccurredAt,
    string? Notes);
