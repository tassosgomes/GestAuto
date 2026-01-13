using GestAuto.Stock.Domain.Enums;

namespace GestAuto.Stock.Application.Vehicles.Dto;

public sealed record VehicleCreate(
    VehicleCategory Category,
    string Vin,
    string Make,
    string Model,
    int YearModel,
    string Color,
    string? Plate,
    string? Trim,
    int? MileageKm,
    Guid? EvaluationId,
    DemoPurpose? DemoPurpose,
    bool IsRegistered);
