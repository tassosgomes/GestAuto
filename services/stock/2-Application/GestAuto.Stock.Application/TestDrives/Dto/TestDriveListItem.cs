namespace GestAuto.Stock.Application.TestDrives.Dto;

public sealed record TestDriveListItem(
    Guid Id,
    Guid LeadId,
    string LeadName,
    string Status,
    DateTime ScheduledAt,
    string VehicleDescription);
