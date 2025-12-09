namespace GestAuto.Commercial.Application.DTOs;

public record ScheduleTestDriveRequest(
    Guid LeadId,
    Guid VehicleId,
    DateTime ScheduledAt,
    string? Notes
);

public record CompleteTestDriveRequest(
    TestDriveChecklistDto Checklist,
    string? CustomerFeedback
);

public record CancelTestDriveRequest(
    string Reason
);

public record TestDriveChecklistDto(
    decimal InitialMileage,
    decimal FinalMileage,
    string FuelLevel,
    string? VisualObservations
);

public record TestDriveResponse(
    Guid Id,
    Guid LeadId,
    Guid VehicleId,
    string Status,
    DateTime ScheduledAt,
    DateTime? CompletedAt,
    Guid SalesPersonId,
    string? Notes,
    TestDriveChecklistResponse? Checklist,
    string? CustomerFeedback,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt
)
{
    public static TestDriveResponse FromEntity(Domain.Entities.TestDrive testDrive) => new(
        testDrive.Id,
        testDrive.LeadId,
        testDrive.VehicleId,
        testDrive.Status.ToString(),
        testDrive.ScheduledAt,
        testDrive.CompletedAt,
        testDrive.SalesPersonId,
        testDrive.Notes,
        testDrive.Checklist != null 
            ? TestDriveChecklistResponse.FromEntity(testDrive.Checklist) 
            : null,
        testDrive.CustomerFeedback,
        testDrive.CancellationReason,
        testDrive.CreatedAt,
        testDrive.UpdatedAt
    );
}

public record TestDriveChecklistResponse(
    decimal InitialMileage,
    decimal FinalMileage,
    string FuelLevel,
    string? VisualObservations
)
{
    public static TestDriveChecklistResponse FromEntity(Domain.ValueObjects.TestDriveChecklist checklist) => new(
        checklist.InitialMileage,
        checklist.FinalMileage,
        checklist.FuelLevel.ToString(),
        checklist.VisualObservations
    );
}

public record TestDriveListItemResponse(
    Guid Id,
    Guid LeadId,
    string LeadName,
    string Status,
    DateTime ScheduledAt,
    string VehicleDescription
)
{
    public static TestDriveListItemResponse FromEntity(Domain.Entities.TestDrive testDrive, string leadName, string vehicleDescription) => new(
        testDrive.Id,
        testDrive.LeadId,
        leadName,
        testDrive.Status.ToString(),
        testDrive.ScheduledAt,
        vehicleDescription
    );
}
