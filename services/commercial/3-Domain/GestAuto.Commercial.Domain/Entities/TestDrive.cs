using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Events;
using GestAuto.Commercial.Domain.ValueObjects;

namespace GestAuto.Commercial.Domain.Entities;

public class TestDrive : BaseEntity
{
    public Guid LeadId { get; private set; }
    public Guid VehicleId { get; private set; }
    public TestDriveStatus Status { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }
    public Guid SalesPersonId { get; private set; }
    public TestDriveChecklist? Checklist { get; private set; }
    public string? CustomerFeedback { get; private set; }
    public string? CancellationReason { get; private set; }

    private TestDrive() { } // EF Core

    public static TestDrive Schedule(
        Guid leadId,
        Guid vehicleId,
        DateTime scheduledAt,
        Guid salesPersonId,
        string? notes = null)
    {
        if (scheduledAt <= DateTime.UtcNow)
            throw new ArgumentException("Scheduled time must be in the future", nameof(scheduledAt));

        if (leadId == Guid.Empty)
            throw new ArgumentException("Lead ID cannot be empty", nameof(leadId));

        if (vehicleId == Guid.Empty)
            throw new ArgumentException("Vehicle ID cannot be empty", nameof(vehicleId));

        if (salesPersonId == Guid.Empty)
            throw new ArgumentException("SalesPerson ID cannot be empty", nameof(salesPersonId));

        var testDrive = new TestDrive
        {
            LeadId = leadId,
            VehicleId = vehicleId,
            Status = TestDriveStatus.Scheduled,
            ScheduledAt = scheduledAt,
            SalesPersonId = salesPersonId,
            Notes = notes
        };

        testDrive.AddEvent(new TestDriveScheduledEvent(testDrive.Id, leadId, vehicleId, scheduledAt));
        return testDrive;
    }

    public void Complete(TestDriveChecklist checklist, string? customerFeedback = null, Guid? completedByUserId = null)
    {
        if (Status != TestDriveStatus.Scheduled)
            throw new InvalidOperationException("Only scheduled test drives can be completed");

        if (checklist == null)
            throw new ArgumentNullException(nameof(checklist), "Checklist is required to complete a test drive");

        Status = TestDriveStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Checklist = checklist;
        CustomerFeedback = customerFeedback;
        UpdatedAt = DateTime.UtcNow;

        AddEvent(new TestDriveCompletedEvent(Id, LeadId));
    }

    public void Cancel(string? reason = null)
    {
        if (Status == TestDriveStatus.Completed)
            throw new InvalidOperationException("Completed test drives cannot be cancelled");

        Status = TestDriveStatus.Cancelled;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }
}