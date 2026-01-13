using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Vehicles.Dto;

public sealed record VehicleListItem(
    Guid Id,
    VehicleCategory Category,
    VehicleStatus CurrentStatus,
    string Vin,
    string? Plate,
    string Make,
    string Model,
    string? Trim,
    int YearModel,
    string Color,
    DateTime CreatedAt,
    DateTime UpdatedAt);
