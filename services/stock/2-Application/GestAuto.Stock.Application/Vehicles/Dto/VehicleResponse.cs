using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Vehicles.Dto;

public sealed record VehicleResponse(
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
    int? MileageKm,
    Guid? EvaluationId,
    DemoPurpose? DemoPurpose,
    bool IsRegistered,
    Guid? CurrentOwnerUserId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
