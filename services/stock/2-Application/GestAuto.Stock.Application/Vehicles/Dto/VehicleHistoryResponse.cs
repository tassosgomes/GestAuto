namespace GestAuto.Stock.Application.Vehicles.Dto;

public sealed record VehicleHistoryResponse(
    Guid VehicleId,
    IReadOnlyList<VehicleHistoryItemResponse> Items);
