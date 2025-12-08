using GestAuto.Commercial.Domain.Entities;
using GestAuto.Commercial.Domain.Enums;
using GestAuto.Commercial.Domain.Events;

namespace GestAuto.Commercial.Domain.Entities;

public class TestDrive : BaseEntity
{
    public Guid LeadId { get; private set; }
    public Guid ProposalId { get; private set; }
    public TestDriveStatus Status { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? Notes { get; private set; }
    public Guid ScheduledBy { get; private set; }

    private TestDrive() { } // EF Core

    public static TestDrive Schedule(
        Guid leadId,
        Guid proposalId,
        DateTime scheduledAt,
        Guid scheduledBy,
        string? notes = null)
    {
        if (scheduledAt <= DateTime.UtcNow)
            throw new ArgumentException("Scheduled time must be in the future", nameof(scheduledAt));

        var testDrive = new TestDrive
        {
            LeadId = leadId,
            ProposalId = proposalId,
            Status = TestDriveStatus.Scheduled,
            ScheduledAt = scheduledAt,
            ScheduledBy = scheduledBy,
            Notes = notes
        };

        testDrive.AddEvent(new TestDriveScheduledEvent(testDrive.Id, leadId, scheduledAt));
        return testDrive;
    }

    public void Complete(string? notes = null)
    {
        if (Status != TestDriveStatus.Scheduled)
            throw new InvalidOperationException("Only scheduled test drives can be completed");

        Status = TestDriveStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Notes = notes ?? Notes;
        UpdatedAt = DateTime.UtcNow;

        AddEvent(new TestDriveCompletedEvent(Id, LeadId));
    }

    public void Cancel(string? notes = null)
    {
        if (Status == TestDriveStatus.Completed)
            throw new InvalidOperationException("Completed test drives cannot be cancelled");

        Status = TestDriveStatus.Cancelled;
        Notes = notes ?? Notes;
        UpdatedAt = DateTime.UtcNow;
    }
}