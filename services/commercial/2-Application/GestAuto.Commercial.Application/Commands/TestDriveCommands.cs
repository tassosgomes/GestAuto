using GestAuto.Commercial.Application.Interfaces;

namespace GestAuto.Commercial.Application.Commands;

public record ScheduleTestDriveCommand(
    Guid LeadId,
    Guid VehicleId,
    DateTime ScheduledAt,
    Guid SalesPersonId,
    string? Notes
) : ICommand<DTOs.TestDriveResponse>;

public record CompleteTestDriveCommand(
    Guid TestDriveId,
    DTOs.TestDriveChecklistDto Checklist,
    string? CustomerFeedback,
    Guid CompletedByUserId
) : ICommand<DTOs.TestDriveResponse>;

public record CancelTestDriveCommand(
    Guid TestDriveId,
    string? Reason,
    Guid CancelledByUserId
) : ICommand<DTOs.TestDriveResponse>;
