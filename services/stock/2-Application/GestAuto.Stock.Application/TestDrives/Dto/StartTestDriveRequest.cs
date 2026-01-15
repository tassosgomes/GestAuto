namespace GestAuto.Stock.Application.TestDrives.Dto;

public sealed record StartTestDriveRequest(
    string? CustomerRef,
    DateTime? StartedAt);
