namespace GestAuto.Stock.Application.Vehicles.Dto;

public sealed record VehicleHistoryItemResponse(
    string Type,
    DateTime OccurredAtUtc,
    Guid UserId,
    IReadOnlyDictionary<string, object?> Details);
