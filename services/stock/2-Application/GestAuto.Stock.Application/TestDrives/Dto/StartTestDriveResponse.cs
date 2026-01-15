namespace GestAuto.Stock.Application.TestDrives.Dto;

public sealed record StartTestDriveResponse(
    Guid TestDriveId,
    Guid VehicleId,
    Guid SalesPersonId,
    string? CustomerRef,
    DateTime StartedAtUtc);
